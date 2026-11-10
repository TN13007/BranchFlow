param([ValidateSet('BranchFlow_Dev','BranchFlow_Test')][string]$Database='BranchFlow_Dev', [string]$BaseUrl='https://localhost:7107')
$ErrorActionPreference='Stop'
if ($BaseUrl -notmatch '^https://localhost:\d+$') { throw 'Local HTTPS only.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Set BRANCHFLOW_DEMO_PASSWORD only in process environment.' }
function ReadJson($path,$token) {
    $result=Invoke-RestMethod -Uri "$BaseUrl/$path" -Headers @{Authorization="Bearer $token"}
    foreach ($entry in $result) { $entry }
}
function Assert($condition,$label) {
    if (-not $condition) { throw $label }
    Write-Output "PASS $label"
}
$manifestText = & sqlcmd -S localhost -E -C -b -f 65001 -d $Database -h -1 -w 65535 -y 4000 -Q "SET NOCOUNT ON; SELECT JSON_VALUE(a.AfterData,'$.Case') AS caseName,o.Id,o.BranchId,o.BusinessDate,o.Status,o.TotalAmount FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId WHERE a.EntityName='BF-WBS5-v1' FOR JSON PATH;"
if ($LASTEXITCODE -ne 0) { throw 'Cannot read fixture manifest.' }
$fixtures = ConvertFrom-Json -InputObject ($manifestText -join '')
Assert ($fixtures.Count -eq 4) 'SQL fixture contains exactly four orders'
$sessions = @{}
try {
    foreach ($role in @('owner','systemadmin','branchadmin.01','employee.a')) {
        $body = @{username="demo.$role";password=$env:BRANCHFLOW_DEMO_PASSWORD} | ConvertTo-Json -Compress
        try { $pair = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $body }
        catch {
            if ([int]$_.Exception.Response.StatusCode -ne 429) { throw }
            Write-Output 'Waiting for login rate-limit window.'
            Start-Sleep -Seconds 60
            $pair = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $body
        }
        $sessions[$role]=$pair.accessToken
        $me=ReadJson 'api/auth/me' $pair.accessToken
        Assert ($me.username -eq "demo.$role") "Login $role with existing password"
        $expectedRole=switch ($role) { 'owner' {'OWNER'} 'systemadmin' {'SYSTEM_ADMIN'} 'branchadmin.01' {'BRANCH_ADMIN'} 'employee.a' {'EMPLOYEE'} }
        Assert ($me.role -eq $expectedRole) "Role $role equals $expectedRole"
    }
    $branches=@(ReadJson 'api/management/branches' $sessions.systemadmin)
    Assert (@($branches | Where-Object code -Like 'BF-DEMO-*').Count -eq 3) 'Three demo branches readable by API'
    $date=$fixtures[0].businessDate
    $revenue=@(ReadJson "api/management/revenue?from=$date&to=$date" $sessions.systemadmin)
    $sqlAmount = & sqlcmd -S localhost -E -C -b -d $Database -h -1 -W -Q "SET NOCOUNT ON; SELECT COALESCE(SUM(TotalAmount),0) FROM dbo.SalesOrder WHERE Status='COMPLETED' AND CONVERT(date,DATEADD(hour,7,CreatedAt))='$date';"
    Assert (($revenue | Measure-Object amount -Sum).Sum -eq [decimal](($sqlAmount -join '').Trim())) 'API daily revenue equals SQL including pre-existing orders'
    foreach ($fixture in $fixtures) {
        $order=ReadJson "api/management/orders/$($fixture.id)" $sessions.owner
        Assert ($order.status -eq $fixture.status -and $order.totalAmount -eq $fixture.totalAmount) "Order snapshot $($fixture.caseName) matches SQL"
        if ($fixture.caseName -Like '*-a') {
            $detail=ReadJson "api/orders/$($fixture.id)" $sessions.'employee.a'
            Assert ($detail.status -eq $fixture.status) "Employee A reads own $($fixture.caseName)"
        }
    }
    foreach ($branch in @($branches | Where-Object code -Like 'BF-DEMO-*')) {
        $stock=@(ReadJson "api/management/branches/$($branch.id)/stock" $sessions.owner)
        $row=$stock | Where-Object name -eq 'WBS5 - Cốt trà đào'
        $expected=switch ($branch.code) { 'BF-DEMO-01' {7} 'BF-DEMO-02' {15} 'BF-DEMO-03' {4} }
        Assert ($row.quantity -eq $expected) "Stock $($branch.code) equals $expected"
    }
    $scope=@(ReadJson 'api/branches/accessible' $sessions.'branchadmin.01')
    Assert ($scope.Count -eq 1 -and $scope[0].code -eq 'BF-DEMO-01') 'Branch Admin remains limited to assigned branch'
    $menu=ReadJson "api/order-menu?branchId=$($scope[0].id)" $sessions.'employee.a'
    Assert (@($menu.categories.products | Where-Object name -eq 'Trà đào cam sả').Count -eq 1) 'Sample drink is in live employee menu'
    Write-Output 'WBS demo API/SQL read verification completed; UI review is separate.'
}
finally {
    foreach ($token in $sessions.Values) { Invoke-RestMethod -Uri "$BaseUrl/api/auth/logout" -Method Post -Headers @{Authorization="Bearer $token"} | Out-Null }
}
