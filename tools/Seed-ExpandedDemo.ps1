param(
    [ValidatePattern('^(localhost|127\.0\.0\.1|\.)(\\[A-Za-z0-9_]+)?$')][string]$Server='localhost\SQLEXPRESS',
    [ValidateSet('BranchFlow_Dev','BranchFlow_Test')][string]$Database='BranchFlow_Dev',
    [switch]$Apply
)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$sql=Get-Content -LiteralPath (Join-Path $root 'database/006-expanded-demo.sql') -Raw
if ($Apply) { $sql=$sql.Replace('DECLARE @Apply bit = 0; -- APPLY_MODE','DECLARE @Apply bit = 1; -- APPLY_MODE') }
$temp=Join-Path ([IO.Path]::GetTempPath()) ('branchflow-demo-'+[guid]::NewGuid().ToString('N')+'.sql')
try {
    [IO.File]::WriteAllText($temp,$sql,[Text.UTF8Encoding]::new($true))
    & sqlcmd -S $Server -E -C -b -f 65001 -d $Database -w 240 -i $temp
    if ($LASTEXITCODE -ne 0) { throw 'Seed failed; transaction rolled back. Inspect the error; do not reset DB.' }
} finally {
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp }
}
