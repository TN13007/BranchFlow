param(
    [ValidateSet('Preview','Apply','Verify')][string]$Mode = 'Preview'
)

$ErrorActionPreference = 'Stop'
$server = 'branchflow-team-sql-khang153.database.windows.net'
$database = 'BranchFlow_GroupDemo'
if (-not $env:SQLCMDPASSWORD) { throw 'Set SQLCMDPASSWORD in this process.' }

$root = Join-Path $PSScriptRoot '../database'
$guard = "IF DB_NAME() <> N'$database' OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('ServerName')),N'') NOT IN (N'branchflow-team-sql-khang153',N'$server') THROW 51060,N'Wrong Azure demo database',1;"
$files = switch ($Mode) {
    'Preview' { '006-expanded-demo.sql' }
    'Apply' { '004-personal-menu-demo.sql'; '003-demo-menu.sql'; '006-expanded-demo.sql'; 'verify-expanded-demo.sql' }
    'Verify' { 'verify-expanded-demo.sql' }
}
foreach ($file in $files) {
    $sql = Get-Content -LiteralPath (Join-Path $root $file) -Raw -Encoding utf8
    $localGuard = "(?m)^IF DB_NAME\(\) NOT IN \(N'BranchFlow_Dev',\s*N'BranchFlow_Test'\)\s*THROW [^;]*;"
    if (-not [regex]::IsMatch($sql, $localGuard)) { throw "Missing database guard in $file." }
    $sql = $sql -replace $localGuard, $guard
    $sql = $sql -replace "(?m)^IF CONVERT\(nvarchar\(128\),SERVERPROPERTY\('MachineName'\)\) <> CONVERT\(nvarchar\(128\),HOST_NAME\(\)\)\s*THROW [^;]*;", ''
    if ($file -eq '003-demo-menu.sql') {
        $sql = $sql.Replace("(N'Matcha latte',N'/images/menu/matcha.png',52000)", "(N'Matcha latte (demo BranchFlow)',N'/images/menu/matcha.png',52000)")
    }
    if ($Mode -eq 'Apply' -and $file -eq '006-expanded-demo.sql') {
        $sql = $sql.Replace('DECLARE @Apply bit = 0; -- APPLY_MODE', 'DECLARE @Apply bit = 1; -- APPLY_MODE')
    }

    $temp = Join-Path ([IO.Path]::GetTempPath()) ("branchflow-azure-seed-" + [guid]::NewGuid() + '.sql')
    try {
        [IO.File]::WriteAllText($temp, $sql, [Text.UTF8Encoding]::new($true))
        & sqlcmd -S "tcp:$server,1433" -d $database -U bfteamadmin -N -b -l 40 -f 65001 -i $temp
        if ($LASTEXITCODE -ne 0) { throw "Azure demo seed step failed: $file." }
    }
    finally {
        if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp }
    }
}
