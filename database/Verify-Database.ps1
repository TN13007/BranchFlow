param(
    [string]$Server = 'localhost'
)

$ErrorActionPreference = 'Stop'
if ($Server -notmatch '^(localhost|\.|\(local\))(\\[A-Za-z0-9_]+)?$') {
    throw 'Chi cho phep SQL Server local, vi du localhost hoac .\SQLEXPRESS.'
}

$sqlcmdCommand = Get-Command sqlcmd -ErrorAction SilentlyContinue
$sqlcmdPath = if ($sqlcmdCommand) { $sqlcmdCommand.Source } else {
    Join-Path ${env:ProgramFiles} 'Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE'
}
if (-not (Test-Path -LiteralPath $sqlcmdPath)) { throw 'Can cai sqlcmd hoac them vao PATH.' }

$connectionString = "Server=$Server;Database=BranchFlow_Test;Integrated Security=True;TrustServerCertificate=True"

# Moi truy van deu mo ket noi rieng vao DB Test, khong nhan connection string cloud.
function Invoke-TestQuery([string]$Sql) {
    $connection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Sql
        $table = New-Object System.Data.DataTable
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
        [void]$adapter.Fill($table)
        return ,$table
    }
    finally { $connection.Dispose() }
}

& $sqlcmdPath -S $Server -E -C -b -d BranchFlow_Test -i (Join-Path $PSScriptRoot '003-verify-schema.sql')
if ($LASTEXITCODE -ne 0) { throw 'SQL constraint verification failed.' }

$counts = Invoke-TestQuery @'
SELECT (SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped=0) AS Tables,
       (SELECT COUNT(*) FROM sys.columns c JOIN sys.tables t ON c.object_id=t.object_id WHERE t.is_ms_shipped=0) AS Fields,
       (SELECT COUNT(*) FROM sys.foreign_keys) AS ForeignKeys,
       (SELECT COUNT(*) FROM sys.foreign_keys WHERE delete_referential_action<>0 OR is_disabled=1 OR is_not_trusted=1) AS InvalidForeignKeys;
'@
$row = $counts.Rows[0]
if ($row.Tables -ne 35 -or $row.Fields -ne 271 -or $row.ForeignKeys -ne 59 -or $row.InvalidForeignKeys -ne 0) {
    throw 'Metadata count, FK trust or no-cascade verification failed.'
}
Write-Output 'PASS: metadata 35 tables / 271 columns / 59 trusted FK, no cascade delete'

# Doi chieu tung cot va FK voi manifest, tranh truong hop dung so luong nhung sai kieu.
$manifest = Get-Content (Join-Path $PSScriptRoot 'schema-v1.json') -Raw | ConvertFrom-Json
$columns = Invoke-TestQuery @'
SELECT t.name AS TableName,c.name AS ColumnName,
CASE WHEN y.name='timestamp' THEN 'ROWVERSION'
     WHEN y.name IN ('nvarchar','varchar','binary') THEN UPPER(y.name)+'('+CASE WHEN c.max_length=-1 THEN 'MAX' ELSE CONVERT(VARCHAR(10),c.max_length/CASE WHEN y.name='nvarchar' THEN 2 ELSE 1 END) END+')'
     WHEN y.name='decimal' THEN 'DECIMAL('+CONVERT(VARCHAR(10),c.precision)+','+CONVERT(VARCHAR(10),c.scale)+')'
     WHEN y.name='datetime2' THEN 'DATETIME2('+CONVERT(VARCHAR(10),c.scale)+')'
     ELSE UPPER(y.name) END AS SqlType,c.is_nullable AS Nullable,c.is_computed AS Computed,c.is_identity AS IdentityColumn
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types y ON y.user_type_id=c.user_type_id WHERE t.is_ms_shipped=0;
'@
foreach ($tableProperty in $manifest.tables.PSObject.Properties) {
    foreach ($field in $tableProperty.Value) {
        $matching = @($columns.Rows | Where-Object { $_.TableName -eq $tableProperty.Name -and $_.ColumnName -eq $field.name })
        if ($matching.Count -ne 1) { throw "Missing column $($tableProperty.Name).$($field.name)" }
        $column = $matching[0]
        $expectedIdentity = $field.name -eq 'Id' -and $field.sql_type -eq 'INT'
        if ($column.SqlType -ne $field.sql_type -or [bool]$column.Nullable -ne $field.nullable -or [bool]$column.Computed -ne $field.computed -or [bool]$column.IdentityColumn -ne $expectedIdentity) {
            throw "Column metadata mismatch: $($tableProperty.Name).$($field.name)"
        }
    }
}
$foreignKeys = Invoke-TestQuery @'
SELECT OBJECT_NAME(k.parent_object_id) AS TableName,COL_NAME(k.parent_object_id,k.parent_column_id) AS ColumnName,
OBJECT_NAME(k.referenced_object_id)+'.'+COL_NAME(k.referenced_object_id,k.referenced_column_id) AS TargetName
FROM sys.foreign_key_columns k;
'@
foreach ($tableProperty in $manifest.foreign_key_targets.PSObject.Properties) {
    foreach ($field in $tableProperty.Value.PSObject.Properties) {
        $matching = @($foreignKeys.Rows | Where-Object { $_.TableName -eq $tableProperty.Name -and $_.ColumnName -eq $field.Name -and $_.TargetName -eq $field.Value })
        if ($matching.Count -ne 1) { throw "FK target mismatch: $($tableProperty.Name).$($field.Name)" }
    }
}
Write-Output 'PASS: all 271 column types/nullability/computed/identity flags and all 59 FK targets match schema-v1.json'

$branchId = $null
$jobs = @()
try {
    # Khong xoa fixture da co cua lan chay khac; dung lai neu bi trung.
    $created = Invoke-TestQuery @'
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF EXISTS(SELECT 1 FROM dbo.Branch WHERE Code=N'__concurrency_verify')
    THROW 51000,N'Concurrent verification fixture already exists.',1;
INSERT dbo.Branch(Code,Name,Address) VALUES(N'__concurrency_verify',N'Concurrency test',N'Local test only');
SELECT CONVERT(INT,SCOPE_IDENTITY()) AS Id;
'@
    $branchId = [int]$created.Rows[0].Id
    # Nhieu SQL connection cung cap ma; giu transaction 200ms de tao tranh chap khoa that.
    for ($i = 0; $i -lt 12; $i++) {
        $jobs += Start-Job -ArgumentList $Server,$branchId -ScriptBlock {
            param($serverName,$id)
            $connection = New-Object System.Data.SqlClient.SqlConnection "Server=$serverName;Database=BranchFlow_Test;Integrated Security=True;TrustServerCertificate=True"
            try {
                $connection.Open()
                $command = $connection.CreateCommand()
                $command.CommandTimeout = 30
                $command.CommandText = @'
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
DECLARE @Code NVARCHAR(50),@Sequence INT;
EXEC dbo.usp_GetNextOrderCode @BranchId,'20261102',@Code OUTPUT,@Sequence OUTPUT;
WAITFOR DELAY '00:00:00.200';
COMMIT TRANSACTION;
SELECT @Code;
'@
                [void]$command.Parameters.AddWithValue('@BranchId',$id)
                $command.ExecuteScalar()
            }
            finally { $connection.Dispose() }
        }
    }
    $completed = @($jobs | Wait-Job -Timeout 45)
    if ($completed.Count -ne 12 -or @($jobs | Where-Object State -ne 'Completed').Count -ne 0) {
        throw 'Concurrent requests did not all complete.'
    }
    $codes = @($jobs | Receive-Job -ErrorAction Stop)
    $counter = Invoke-TestQuery "SELECT LastNumber FROM dbo.OrderDailyCounter WHERE BranchId=$branchId AND BusinessDate='20261102'"
    if ($codes.Count -ne 12 -or @($codes | Select-Object -Unique).Count -ne 12 -or $counter.Rows[0].LastNumber -ne 12) {
        throw 'Concurrent counter verification failed.'
    }
    Write-Output 'PASS: 12 concurrent SQL connections, 12 unique codes, counter = 12'
}
finally {
    if ($jobs.Count -gt 0) {
        $jobs | Stop-Job
        $jobs | Remove-Job
    }
    if ($null -ne $branchId) {
        # Chi xoa fixture do lan chay nay tao, kiem tra lai code va Id trong DB Test.
        $null = Invoke-TestQuery @"
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF EXISTS(SELECT 1 FROM dbo.Branch WHERE Id=$branchId AND Code=N'__concurrency_verify')
BEGIN
    DELETE dbo.OrderDailyCounter WHERE BranchId=$branchId;
    DELETE dbo.Branch WHERE Id=$branchId AND Code=N'__concurrency_verify';
END;
COMMIT TRANSACTION;
"@
    }
}
Write-Output 'PASS: concurrency fixtures cleaned; verification completed'
