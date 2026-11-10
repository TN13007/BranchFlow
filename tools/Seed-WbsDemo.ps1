param(
    [ValidateSet('BranchFlow_Dev','BranchFlow_Test')][string]$Database = 'BranchFlow_Test',
    [string]$Server = 'localhost'
)
$ErrorActionPreference = 'Stop'
if ($Server -notmatch '^(localhost|\.|127\.0\.0\.1)(\\[A-Za-z0-9_]+)?$') { throw 'Only local SQL Server is allowed.' }
$root = Split-Path $PSScriptRoot -Parent
function Query([string]$Sql) {
    $result = & sqlcmd -S $Server -E -C -b -f 65001 -d $Database -h -1 -W -Q "SET NOCOUNT ON; $Sql"
    if ($LASTEXITCODE -ne 0) { throw 'SQL verification failed.' }
    return ($result -join "`n").Trim()
}
# Chỉ so sánh fingerprint trong bộ nhớ; không in password/hash vào log.
$accountsBefore = Query "SELECT CONVERT(varchar(64),HASHBYTES('SHA2_256',(SELECT Id,Username,PasswordHash,IsActive,Deleted FROM dbo.AppUser ORDER BY Id FOR JSON PATH)),2);"
$rolesBefore = Query "SELECT COUNT(*) FROM dbo.AppUser u JOIN dbo.Role r ON r.Id=u.RoleId WHERE u.Username IN ('demo.owner','demo.systemadmin','demo.branchadmin.01','demo.branchadmin.02','demo.branchadmin.03','demo.employee.a','demo.employee.b','demo.employee.locked','demo.employee.unassigned') AND u.Deleted=0;"
if ($rolesBefore -ne '9') { throw 'Run the existing DemoSeed first; this script never creates or resets passwords.' }
foreach ($script in @('003-demo-menu.sql','005-wbs-demo-data.sql')) {
    & sqlcmd -S $Server -E -C -b -f 65001 -W -w 240 -d $Database -i (Join-Path $root "database/$script")
    if ($LASTEXITCODE -ne 0) { throw "Seed failed: $script. Stop and inspect; do not reset the database." }
}
$accountsAfter = Query "SELECT CONVERT(varchar(64),HASHBYTES('SHA2_256',(SELECT Id,Username,PasswordHash,IsActive,Deleted FROM dbo.AppUser ORDER BY Id FOR JSON PATH)),2);"
if ($accountsBefore -ne $accountsAfter) { throw 'Account data changed unexpectedly.' }
Write-Output "PASS $Database seeded; existing accounts, passwords and account states unchanged."
