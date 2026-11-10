param(
    [string]$BaseUrl = 'https://localhost:7107',
    [ValidatePattern('^(localhost|\.)(\\[A-Za-z0-9_-]+)?$')][string]$SqlServer = 'localhost'
)

$ErrorActionPreference = 'Stop'
if ($BaseUrl -notmatch '^https://localhost:\d+$') { throw 'Local HTTPS API only.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Set BRANCHFLOW_DEMO_PASSWORD in this process.' }
$checks = 0
function Check([bool]$ok, [string]$label) {
    if (-not $ok) { throw "FAIL $label" }
    $script:checks++
    Write-Output "PASS $label"
}
function Sql([string]$query) {
    $value = & sqlcmd -S $SqlServer -E -C -b -d BranchFlow_Test -h -1 -W -Q "SET NOCOUNT ON; $query"
    if ($LASTEXITCODE -ne 0) { throw 'SQL query failed in BranchFlow_Test' }
    return ($value -join '').Trim()
}
function Api([string]$method, [string]$path, [string]$token = '', [object]$body = $null) {
    $args = @{ Uri = "$BaseUrl/$path"; Method = $method; SkipHttpErrorCheck = $true; ErrorAction = 'Stop' }
    if ($token) { $args.Headers = @{ Authorization = "Bearer $token" } }
    if ($null -ne $body) { $args.ContentType = 'application/json'; $args.Body = ConvertTo-Json $body -Depth 8 -Compress }
    $response = Invoke-WebRequest @args
    $data = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
    return [pscustomobject]@{ Status = [int]$response.StatusCode; Body = $data }
}

$token = ''
try {
    Check ((Sql 'SELECT DB_NAME();') -eq 'BranchFlow_Test') 'SQL targets BranchFlow_Test'
    $login = Api POST 'api/auth/login' '' @{ username = 'demo.branchadmin.01'; password = $env:BRANCHFLOW_DEMO_PASSWORD }
    if ($login.Status -eq 429) {
        Write-Output 'Waiting for local login rate-limit window.'
        Start-Sleep -Seconds 60
        $login = Api POST 'api/auth/login' '' @{ username = 'demo.branchadmin.01'; password = $env:BRANCHFLOW_DEMO_PASSWORD }
    }
    Check ($login.Status -eq 200 -and $login.Body.accessToken) 'Branch Admin login'
    $token = [string]$login.Body.accessToken
    $branches = Api GET 'api/branches/accessible' $token
    Check ($branches.Status -eq 200 -and @($branches.Body).Count -eq 1 -and $branches.Body[0].code -eq 'BF-DEMO-01') 'Branch Admin scoped to Demo 01'
    $branchId = [int]$branches.Body[0].id
    $stockPath = "api/management/branches/$branchId/stock"
    $stock = Api GET $stockPath $token
    $row = @($stock.Body | Where-Object name -eq 'WBS5 - Cốt trà đào' | Select-Object -First 1)[0]
    Check ($stock.Status -eq 200 -and $row.quantity -eq 7 -and $row.rowVersion) 'Demo stock starts at seven with RowVersion'
    $ingredientId = [int]$row.ingredientId
    $beforeCount = [int](Sql "SELECT COUNT(*) FROM dbo.StockTransactionDetail d JOIN dbo.StockTransaction t ON t.Id=d.StockTransactionId WHERE t.BranchId=$branchId AND d.IngredientId=$ingredientId;")
    $initialVersion = [string]$row.rowVersion
    $in = Api POST 'api/management/stock' $token @{ branchId = $branchId; type = 'IN'; reason = 'QA HTTP stock import 10/11'; lines = @(@{
        ingredientId = $ingredientId; quantity = 1; rowVersion = $initialVersion
    }) }
    Check ($in.Status -eq 200) 'Import one unit succeeds'
    $stock = Api GET $stockPath $token
    $row = @($stock.Body | Where-Object ingredientId -eq $ingredientId | Select-Object -First 1)[0]
    Check ($row.quantity -eq 8 -and $row.rowVersion -ne $initialVersion) 'Stock becomes eight and RowVersion changes'
    $out = Api POST 'api/management/stock' $token @{ branchId = $branchId; type = 'OUT'; reason = 'QA HTTP stock export 10/11'; lines = @(@{
        ingredientId = $ingredientId; quantity = 1; rowVersion = [string]$row.rowVersion
    }) }
    Check ($out.Status -eq 200) 'Export one unit succeeds'
    $stock = Api GET $stockPath $token
    $row = @($stock.Body | Where-Object ingredientId -eq $ingredientId | Select-Object -First 1)[0]
    Check ($row.quantity -eq 7) 'Stock returns to seven'
    $overdraw = Api POST 'api/management/stock' $token @{ branchId = $branchId; type = 'OUT'; reason = 'QA overdraw denied'; lines = @(@{
        ingredientId = $ingredientId; quantity = 8; rowVersion = [string]$row.rowVersion
    }) }
    Check ($overdraw.Status -eq 400) 'Overdraw returns HTTP 400'
    $stale = Api POST 'api/management/stock' $token @{ branchId = $branchId; type = 'OUT'; reason = 'QA stale version denied'; lines = @(@{
        ingredientId = $ingredientId; quantity = 1; rowVersion = $initialVersion
    }) }
    Check ($stale.Status -eq 409) 'Stale RowVersion returns HTTP 409'
    $afterCount = [int](Sql "SELECT COUNT(*) FROM dbo.StockTransactionDetail d JOIN dbo.StockTransaction t ON t.Id=d.StockTransactionId WHERE t.BranchId=$branchId AND d.IngredientId=$ingredientId;")
    Check ($afterCount -eq $beforeCount + 2) 'Only two successful stock transactions persist'
    Check ((Sql "SELECT Quantity FROM dbo.BranchIngredient WHERE BranchId=$branchId AND IngredientId=$ingredientId;") -eq '7.000') 'SQL balance remains seven after denied writes'
    $history = Api GET "api/management/branches/$branchId/stock/history" $token
    Check ($history.Status -eq 200 -and @($history.Body | Where-Object id -In @($in.Body, $out.Body)).Count -eq 2) 'Both successful transactions appear in history'
    Write-Output "Stock HTTP/SQL checks: $checks passed. Two QA transactions remain in BranchFlow_Test; balance restored to seven."
}
finally {
    if ($token) { try { Api POST 'api/auth/logout' $token | Out-Null } catch { } }
}
