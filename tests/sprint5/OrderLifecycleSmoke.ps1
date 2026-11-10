param(
    [string]$BaseUrl = 'https://localhost:7107',
    [ValidatePattern('^(localhost|\.)(\\[A-Za-z0-9_-]+)?$')][string]$SqlServer = 'localhost'
)

$ErrorActionPreference = 'Stop'
if ($BaseUrl -notmatch '^https://localhost:\d+$') { throw 'Local HTTPS API only.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Set BRANCHFLOW_DEMO_PASSWORD in this process.' }
$Database = 'BranchFlow_Test'
$tokens = @{}
$created = $null
$checks = 0

function Check([bool]$ok, [string]$label) {
    if (-not $ok) { throw "FAIL $label" }
    $script:checks++
    Write-Output "PASS $label"
}
function Sql([string]$query) {
    $value = & sqlcmd -S $SqlServer -E -C -b -d $Database -h -1 -W -Q "SET NOCOUNT ON; $query"
    if ($LASTEXITCODE -ne 0) { throw "SQL query failed in $Database" }
    return ($value -join '').Trim()
}
function Api([string]$method, [string]$path, [string]$token = '', [object]$body = $null) {
    $args = @{ Uri = "$BaseUrl/$path"; Method = $method; SkipHttpErrorCheck = $true; ErrorAction = 'Stop' }
    if ($token) { $args.Headers = @{ Authorization = "Bearer $token" } }
    if ($null -ne $body) { $args.ContentType = 'application/json'; $args.Body = ConvertTo-Json $body -Depth 10 -Compress }
    $response = Invoke-WebRequest @args
    $data = if ($response.Content) { $response.Content | ConvertFrom-Json } else { $null }
    return [pscustomobject]@{ Status = [int]$response.StatusCode; Body = $data }
}
function Login([string]$name) {
    $response = Api POST 'api/auth/login' '' @{ username = "demo.$name"; password = $env:BRANCHFLOW_DEMO_PASSWORD }
    if ($response.Status -eq 429) {
        Write-Output 'Waiting for local login rate-limit window.'
        Start-Sleep -Seconds 60
        $response = Api POST 'api/auth/login' '' @{ username = "demo.$name"; password = $env:BRANCHFLOW_DEMO_PASSWORD }
    }
    Check ($response.Status -eq 200 -and $response.Body.accessToken) "Login $name"
    $tokens[$name] = [string]$response.Body.accessToken
}

try {
    Check ((Sql 'SELECT DB_NAME();') -eq $Database) 'SQL targets BranchFlow_Test'
    foreach ($name in @('employee.a', 'employee.b', 'branchadmin.01', 'branchadmin.02')) { Login $name }
    $branches = Api GET 'api/branches/accessible' $tokens.'employee.a'
    Check ($branches.Status -eq 200 -and @($branches.Body).Count -eq 1 -and $branches.Body[0].code -eq 'BF-DEMO-01') 'Employee A scoped to Demo 01'
    $branchId = [int]$branches.Body[0].id
    $menu = Api GET "api/order-menu?branchId=$branchId" $tokens.'employee.a'
    Check ($menu.Status -eq 200) 'Employee A menu reads from API'
    $drink = @($menu.Body.categories.products | Where-Object name -eq 'Trà đào cam sả' | Select-Object -First 1)[0]
    $size = @($drink.productSizes | Where-Object sizeName -eq 'M' | Select-Object -First 1)[0]
    $pearl = @($menu.Body.toppingGroups.toppings | Where-Object name -eq 'Trân châu' | Select-Object -First 1)[0]
    $jelly = @($menu.Body.toppingGroups.toppings | Where-Object name -eq 'Thạch' | Select-Object -First 1)[0]
    Check ($size.id -gt 0 -and $pearl.id -gt 0 -and $jelly.id -gt 0) 'Priced size and two toppings available'
    $unit = [decimal]$size.price + 3 * [decimal]$pearl.unitPrice + [decimal]$jelly.unitPrice
    $stockBefore = [decimal](Sql "SELECT Quantity FROM dbo.BranchIngredient WHERE BranchId=$branchId AND IngredientId=(SELECT Id FROM dbo.Ingredient WHERE Name=N'WBS5 - Cốt trà đào');")
    $businessDate = (Get-Date).ToString('yyyy-MM-dd')
    $revenueBefore = Api GET "api/management/revenue?branchId=$branchId&from=$businessDate&to=$businessDate" $tokens.'branchadmin.01'
    Check ($revenueBefore.Status -eq 200) 'Branch Admin revenue baseline available'
    $baseline = [decimal](($revenueBefore.Body | Measure-Object amount -Sum).Sum)
    $newOrder = Api POST 'api/orders' $tokens.'employee.a' @{ branchId = $branchId; items = @(@{
        productSizeId = [int]$size.id; quantity = 1; toppings = @(
            @{ toppingId = [int]$pearl.id; quantity = 3 }, @{ toppingId = [int]$jelly.id; quantity = 1 }
        ); noteOptionIds = @()
    }) }
    Check ($newOrder.Status -eq 201 -and $newOrder.Body.totalAmount -eq $unit) 'Create one cup with two toppings and DB price'
    $created = $newOrder.Body
    $id = [int]$created.id
    Check ((Sql "SELECT COUNT(*) FROM dbo.SalesOrder WHERE Id=$id AND Code=N'$($created.code)' AND TotalAmount=$unit AND Status=N'COMPLETED';") -eq '1') 'Created order matches SQL once'
    $view = Api GET "api/management/orders/$id" $tokens.'branchadmin.01'
    Check ($view.Status -eq 200 -and $view.Body.items.Count -eq 1) 'Branch Admin reads order and line'
    $lineId = [int]$view.Body.items[0].id
    $change = @{ reason = 'QA chỉnh hai ly'; rowVersion = [string]$view.Body.rowVersion; cancel = $false;
        items = @(@{ itemId = $lineId; quantity = 2 }) }
    $deniedEmployee = Api PUT "api/management/orders/$id" $tokens.'employee.a' $change
    Check ($deniedEmployee.Status -eq 403) 'Employee cannot adjust order'
    $deniedBranch = Api PUT "api/management/orders/$id" $tokens.'branchadmin.02' $change
    Check ($deniedBranch.Status -eq 403) 'Foreign Branch Admin cannot adjust order'
    $bad = $change.Clone(); $bad.items = @(@{ itemId = $lineId; quantity = 0 })
    Check ((Api PUT "api/management/orders/$id" $tokens.'branchadmin.01' $bad).Status -eq 400) 'Zero cups rejected'
    Check ((Api PUT "api/management/orders/$id" $tokens.'branchadmin.01' $change).Status -eq 200) 'Branch Admin changes quantity to two'
    $afterChange = Api GET "api/management/orders/$id" $tokens.'branchadmin.01'
    Check ($afterChange.Body.totalAmount -eq 2 * $unit -and $afterChange.Body.items[0].quantity -eq 2 -and $afterChange.Body.history.Count -eq 1) 'New total and adjustment history match'
    Check ((Sql "SELECT COUNT(*) FROM dbo.OrderAdjustment WHERE SalesOrderId=$id;") -eq '1') 'One adjustment persisted in SQL'
    $revenueWithOrder = Api GET "api/management/revenue?branchId=$branchId&from=$businessDate&to=$businessDate" $tokens.'branchadmin.01'
    $amountWithOrder = [decimal](($revenueWithOrder.Body | Measure-Object amount -Sum).Sum)
    Check ($amountWithOrder -eq $baseline + 2 * $unit) 'Revenue uses adjusted completed amount'
    $cancel = @{ reason = 'QA hủy đơn thử'; rowVersion = [string]$afterChange.Body.rowVersion; cancel = $true; items = @() }
    Check ((Api PUT "api/management/orders/$id" $tokens.'branchadmin.01' $cancel).Status -eq 200) 'Branch Admin cancels with reason'
    $afterCancel = Api GET "api/management/orders/$id" $tokens.'branchadmin.01'
    Check ($afterCancel.Body.status -eq 'CANCELLED' -and $afterCancel.Body.history.Count -eq 2) 'Cancelled status and two history entries visible'
    Check ((Api PUT "api/management/orders/$id" $tokens.'branchadmin.01' $cancel).Status -eq 409) 'Cancelled order cannot be adjusted again'
    $revenueFinal = Api GET "api/management/revenue?branchId=$branchId&from=$businessDate&to=$businessDate" $tokens.'branchadmin.01'
    Check ([decimal](($revenueFinal.Body | Measure-Object amount -Sum).Sum) -eq $baseline) 'Cancelled order excluded from revenue'
    Check ((Sql "SELECT COUNT(*) FROM dbo.OrderAdjustment WHERE SalesOrderId=$id;") -eq '2') 'Two history rows persisted in SQL'
    $mine = Api GET "api/orders/$id" $tokens.'employee.a'
    Check ($mine.Status -eq 200 -and $mine.Body.status -eq 'CANCELLED') 'Employee A sees final status of own order'
    Check ((Api GET "api/orders/$id" $tokens.'employee.b').Status -eq 404) 'Employee B cannot see another employee order'
    $stockAfter = [decimal](Sql "SELECT Quantity FROM dbo.BranchIngredient WHERE BranchId=$branchId AND IngredientId=(SELECT Id FROM dbo.Ingredient WHERE Name=N'WBS5 - Cốt trà đào');")
    Check ($stockAfter -eq $stockBefore) 'Order create/adjust/cancel did not auto-deduct stock'
    Write-Output "Order lifecycle checks: $checks passed. Test DB evidence order: $($created.code)."
}
finally {
    foreach ($token in $tokens.Values) { try { Api POST 'api/auth/logout' $token | Out-Null } catch { } }
}
