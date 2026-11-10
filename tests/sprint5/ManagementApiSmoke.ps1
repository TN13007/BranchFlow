param(
    [string]$BaseUrl = 'https://localhost:7107',
    [string]$Server = 'localhost'
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'PowerShell 7+ is required.' }
if ($BaseUrl -notmatch '^https://localhost:\d+$') { throw 'Only a local HTTPS API is allowed.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Set BRANCHFLOW_DEMO_PASSWORD in the process environment; never put it in this script or a log.' }
if ($Server -notmatch '^(localhost|\.|127\.0\.0\.1)(\\[A-Za-z0-9_]+)?$') { throw 'Only a local SQL Server is allowed.' }

$Database = 'BranchFlow_Test'
$RunId = [Guid]::NewGuid().ToString('N').Substring(0, 10).ToUpperInvariant()
$BranchA = "QA110A-$RunId"
$BranchB = "QA110B-$RunId"
$AdminA = "qa110.admin.$($RunId.ToLowerInvariant())"
$AdminB = "qa110.admin2.$($RunId.ToLowerInvariant())"
$EmployeeA = "qa110.employee.$($RunId.ToLowerInvariant())"
$Results = [System.Collections.Generic.List[object]]::new()
$Tokens = @{}
$TempPasswords = @{}

function Write-Case([string]$Name, [string]$Expected, [string]$Actual, [bool]$Passed) {
    $Results.Add([pscustomobject]@{
        Case = $Name
        Expected = $Expected
        Actual = $Actual
        Result = if ($Passed) { 'PASS' } else { 'FAIL' }
    })
    if (-not $Passed) { throw "$Name expected $Expected but received $Actual." }
    Write-Output "PASS $Name ($Actual)"
}

function Invoke-Api([string]$Method, [string]$Path, [string]$Token = '', [object]$Body = $null) {
    $params = @{
        Uri = "$BaseUrl/$Path"
        Method = $Method
        SkipCertificateCheck = $true
        ErrorAction = 'Stop'
    }
    if ($Token) { $params.Headers = @{ Authorization = "Bearer $Token" } }
    if ($null -ne $Body) {
        $params.ContentType = 'application/json'
        $params.Body = ConvertTo-Json -InputObject $Body -Depth 12 -Compress
    }
    try {
        $value = Invoke-RestMethod @params
        return [pscustomobject]@{ Status = 200; Body = $value }
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) { throw }
        $status = [int]$response.StatusCode
        $raw = ''
        try {
            if ($response.Content) {
                $raw = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            }
            elseif ($response.GetResponseStream()) {
                $reader = [System.IO.StreamReader]::new($response.GetResponseStream())
                try { $raw = $reader.ReadToEnd() } finally { $reader.Dispose() }
            }
        } catch { }
        $parsed = $raw
        if ($raw) { try { $parsed = ConvertFrom-Json -InputObject $raw } catch { } }
        return [pscustomobject]@{ Status = $status; Body = $parsed }
    }
}

function Assert-Status($Response, [int]$Expected, [string]$Name) {
    Write-Case $Name ([string]$Expected) ([string]$Response.Status) ($Response.Status -eq $Expected)
}

function Login([string]$Username, [string]$Role, [string]$Password = $env:BRANCHFLOW_DEMO_PASSWORD) {
    $response = Invoke-Api 'POST' 'api/auth/login' '' @{ username = $Username; password = $Password }
    if ($response.Status -eq 429) {
        Write-Output "Login rate limit reached for $Role; waiting for the configured one-minute window."
        Start-Sleep -Seconds 60
        $response = Invoke-Api 'POST' 'api/auth/login' '' @{ username = $Username; password = $Password }
    }
    Assert-Status $response 200 "Login $Role"
    if (-not $response.Body.accessToken) { throw "Login $Role did not return an access token." }
    $Tokens[$Role] = [string]$response.Body.accessToken
}

function Sql([string]$Query) {
    $output = & sqlcmd -S $Server -E -C -b -f 65001 -d $Database -h -1 -W -Q "SET NOCOUNT ON; $Query"
    if ($LASTEXITCODE -ne 0) { throw 'SQL query failed; inspect the local Test database.' }
    return (($output -join [Environment]::NewLine).Trim())
}

function New-TemporaryPassword {
    return "Q!a-$([Guid]::NewGuid().ToString('N'))-9Z"
}

function Get-StockSignature([int]$BranchId) {
    return Sql "SELECT CONCAT((SELECT COUNT_BIG(*) FROM dbo.StockTransaction WHERE BranchId=$BranchId),N'|',(SELECT COUNT_BIG(*) FROM dbo.AuditLog WHERE BranchId=$BranchId AND EntityName=N'StockTransaction'),N'|',(SELECT COUNT_BIG(*) FROM dbo.BranchIngredient WHERE BranchId=$BranchId),N'|',(SELECT COALESCE(SUM(Quantity),0) FROM dbo.BranchIngredient WHERE BranchId=$BranchId));"
}

function Get-ProductSignature([int]$ProductId) {
    return Sql "SELECT CONCAT(p.Name,N'|',CONVERT(varchar(1),p.IsActive),N'|',COALESCE((SELECT STRING_AGG(CONCAT(CONVERT(varchar(20),ps.Id),N':',CONVERT(varchar(40),ps.Price),N':',CONVERT(varchar(1),ps.IsActive)),N',') WITHIN GROUP (ORDER BY ps.Id) FROM dbo.ProductSize ps WHERE ps.ProductId=p.Id),N'')) FROM dbo.Product p WHERE p.Id=$ProductId;"
}

function Assert-ApiAndSqlBranch([int]$Id, [string]$Code, [string]$ExpectedName, [string]$ExpectedAddress, [string]$Token) {
    $api = Invoke-Api 'GET' 'api/management/branches' $Token
    Assert-Status $api 200 "Read branch list $Code"
    $row = @($api.Body | Where-Object { $_.code -eq $Code }) | Select-Object -First 1
    if (-not $row) { throw "API did not return $Code." }
    Write-Case "API branch name $Code" $ExpectedName ([string]$row.name) ($row.name -eq $ExpectedName)
    $sql = Sql "SELECT CONCAT(Name,N'|',Address,N'|',CONVERT(varchar(1),IsActive)) FROM dbo.Branch WHERE Id=$Id AND Code=N'$Code';"
    $expected = "$ExpectedName|$ExpectedAddress|1"
    Write-Case "SQL branch matches API $Code" $expected $sql ($sql -eq $expected)
}

$cleanupSql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @branchIds TABLE (Id int PRIMARY KEY);
INSERT @branchIds SELECT Id FROM dbo.Branch WHERE Code IN (N'$BranchA',N'$BranchB');
DECLARE @userIds TABLE (Id int PRIMARY KEY);
INSERT @userIds SELECT Id FROM dbo.AppUser WHERE Username IN (N'$AdminA',N'$AdminB',N'$EmployeeA');
DECLARE @profileIds TABLE (Id int PRIMARY KEY);
INSERT @profileIds SELECT Id FROM dbo.EmployeeProfile WHERE UserId IN (SELECT Id FROM @userIds);
DELETE FROM dbo.RefreshToken WHERE SessionId IN (SELECT Id FROM dbo.AuthSession WHERE UserId IN (SELECT Id FROM @userIds));
DELETE FROM dbo.AuthSession WHERE UserId IN (SELECT Id FROM @userIds);
DELETE FROM dbo.EmployeeStockPermission WHERE EmployeeProfileId IN (SELECT Id FROM @profileIds);
DELETE FROM dbo.EmployeeBranchAssignment WHERE EmployeeProfileId IN (SELECT Id FROM @profileIds);
DELETE FROM dbo.AdminBranchAssignment WHERE AdminUserId IN (SELECT Id FROM @userIds) OR BranchId IN (SELECT Id FROM @branchIds);
DELETE FROM dbo.EmployeeProfile WHERE Id IN (SELECT Id FROM @profileIds);
DELETE FROM dbo.AppUser WHERE Id IN (SELECT Id FROM @userIds);
DELETE FROM dbo.Branch WHERE Id IN (SELECT Id FROM @branchIds);
COMMIT TRANSACTION;
"@

try {
    if ((Sql 'SELECT DB_NAME();') -ne $Database) { throw "SQL connection must target $Database." }

    Login 'demo.systemadmin' 'SYSTEM_ADMIN'
    Login 'demo.owner' 'OWNER'
    Login 'demo.branchadmin.01' 'BRANCH_ADMIN_01'
    Login 'demo.employee.a' 'EMPLOYEE_A'

    $unauth = Invoke-Api 'GET' 'api/management/branches'
    Assert-Status $unauth 401 'Missing bearer token is rejected'

    $systemBranches = Invoke-Api 'GET' 'api/management/branches' $Tokens.SYSTEM_ADMIN
    Assert-Status $systemBranches 200 'SystemAdmin reads all branches'
    $demo01 = @($systemBranches.Body | Where-Object code -eq 'BF-DEMO-01') | Select-Object -First 1
    $demo02 = @($systemBranches.Body | Where-Object code -eq 'BF-DEMO-02') | Select-Object -First 1
    if (-not $demo01 -or -not $demo02) { throw 'Expected BF-DEMO-01 and BF-DEMO-02 in the test database.' }

    $ownerBranches = Invoke-Api 'GET' 'api/management/branches' $Tokens.OWNER
    Assert-Status $ownerBranches 200 'Owner reads permitted branch data'
    $demoCount = @($ownerBranches.Body | Where-Object code -like 'BF-DEMO-*').Count
    Write-Case 'Owner receives all accessible demo branches' 'at least 3' ([string]$demoCount) ($demoCount -ge 3)
    Assert-Status (Invoke-Api 'GET' 'api/management/users' $Tokens.OWNER) 403 'Owner cannot read management user directory'

    $forbiddenBranch = @{ code = "DENY-$RunId"; name = 'Must not be created by Owner'; address = 'Local test'; isActive = $true }
    Assert-Status (Invoke-Api 'POST' 'api/management/branches' $Tokens.OWNER $forbiddenBranch) 403 'Owner cannot create branch'
    Assert-Status (Invoke-Api 'PUT' "api/management/branches/$($demo01.id)" $Tokens.OWNER @{
        code = $demo01.code; name = 'Must not be updated by Owner'; address = $demo01.address; isActive = $demo01.isActive
    }) 403 'Owner cannot update branch'
    Write-Case 'Owner create request leaves no branch in SQL' '0' (Sql "SELECT COUNT(*) FROM dbo.Branch WHERE Code=N'DENY-$RunId';") ((Sql "SELECT COUNT(*) FROM dbo.Branch WHERE Code=N'DENY-$RunId';") -eq '0')

    $branchBodyA = @{ code = $BranchA; name = 'QA Temporary Branch A'; address = 'Local test A'; isActive = $true }
    $branchBodyB = @{ code = $BranchB; name = 'QA Temporary Branch B'; address = 'Local test B'; isActive = $true }
    $createdA = Invoke-Api 'POST' 'api/management/branches' $Tokens.SYSTEM_ADMIN $branchBodyA
    Assert-Status $createdA 200 'SystemAdmin creates branch A'
    $createdB = Invoke-Api 'POST' 'api/management/branches' $Tokens.SYSTEM_ADMIN $branchBodyB
    Assert-Status $createdB 200 'SystemAdmin creates branch B'
    $branchIdA = [int]$createdA.Body
    $branchIdB = [int]$createdB.Body
    if ($branchIdA -le 0 -or $branchIdB -le 0) { throw 'Create branch returned an invalid ID.' }

    $branchBodyA.name = 'QA Temporary Branch A Updated'
    $branchBodyA.address = 'Local test A Updated'
    Assert-Status (Invoke-Api 'PUT' "api/management/branches/$branchIdA" $Tokens.SYSTEM_ADMIN $branchBodyA) 200 'SystemAdmin updates branch A'
    Assert-ApiAndSqlBranch $branchIdA $BranchA $branchBodyA.name $branchBodyA.address $Tokens.SYSTEM_ADMIN

    $TempPasswords.AdminA = New-TemporaryPassword
    $branchAdminCreate = Invoke-Api 'POST' 'api/management/users' $Tokens.SYSTEM_ADMIN @{
        username = $AdminA; password = $TempPasswords.AdminA; fullName = 'QA Temporary Branch Admin'
        role = 'BRANCH_ADMIN'; branchId = $branchIdA
    }
    Assert-Status $branchAdminCreate 200 'SystemAdmin creates assigned BranchAdmin'
    $branchAdminId = [int]$branchAdminCreate.Body

    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.SYSTEM_ADMIN @{
        userId = $branchAdminId; branchId = $branchIdB
    }) 200 'One BranchAdmin can be assigned to multiple branches'

    $TempPasswords.AdminB = New-TemporaryPassword
    $secondAdminCreate = Invoke-Api 'POST' 'api/management/users' $Tokens.SYSTEM_ADMIN @{
        username = $AdminB; password = $TempPasswords.AdminB; fullName = 'QA Duplicate Branch Admin'
        role = 'BRANCH_ADMIN'
    }
    Assert-Status $secondAdminCreate 200 'SystemAdmin creates unassigned second BranchAdmin'
    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.SYSTEM_ADMIN @{
        userId = [int]$secondAdminCreate.Body; branchId = $branchIdA
    }) 409 'Second effective BranchAdmin for the same branch is rejected'
    $adminAssignments = Sql "SELECT COUNT(*) FROM dbo.AdminBranchAssignment WHERE BranchId=$branchIdA AND EffectiveTo IS NULL;"
    Write-Case 'SQL keeps exactly one open Admin assignment' '1' $adminAssignments ($adminAssignments -eq '1')

    $TempPasswords.EmployeeA = New-TemporaryPassword
    $employeeCreate = Invoke-Api 'POST' 'api/management/users' $Tokens.SYSTEM_ADMIN @{
        username = $EmployeeA; password = $TempPasswords.EmployeeA; fullName = 'QA Temporary Employee'
        role = 'EMPLOYEE'; branchId = $branchIdA
    }
    Assert-Status $employeeCreate 200 'SystemAdmin creates Employee with initial assignment'
    $employeeId = [int]$employeeCreate.Body
    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.SYSTEM_ADMIN @{
        userId = $employeeId; branchId = $branchIdB
    }) 409 'Second effective Employee assignment is rejected'
    $employeeAssignments = Sql "SELECT COUNT(*) FROM dbo.EmployeeBranchAssignment ea JOIN dbo.EmployeeProfile ep ON ep.Id=ea.EmployeeProfileId JOIN dbo.AppUser u ON u.Id=ep.UserId WHERE u.Username=N'$EmployeeA' AND ea.EffectiveTo IS NULL;"
    Write-Case 'SQL keeps exactly one open Employee assignment' '1' $employeeAssignments ($employeeAssignments -eq '1')

    Login $AdminA 'TEMP_BRANCH_ADMIN' $TempPasswords.AdminA
    $scopedAdminBranches = Invoke-Api 'GET' 'api/management/branches' $Tokens.TEMP_BRANCH_ADMIN
    Assert-Status $scopedAdminBranches 200 'Temporary BranchAdmin reads branch list'
    Write-Case 'BranchAdmin with two assignments sees exactly those two branches' "$BranchA,$BranchB" ((@($scopedAdminBranches.Body | Select-Object -ExpandProperty code | Sort-Object)) -join ',') (@($scopedAdminBranches.Body).Count -eq 2 -and @($scopedAdminBranches.Body | Where-Object code -in @($BranchA,$BranchB)).Count -eq 2)

    $scopedUsers = Invoke-Api 'GET' 'api/management/users' $Tokens.TEMP_BRANCH_ADMIN
    Assert-Status $scopedUsers 200 'BranchAdmin reads Employee directory in assigned branches'
    $tempEmployee = @($scopedUsers.Body | Where-Object username -eq $EmployeeA) | Select-Object -First 1
    if (-not $tempEmployee) { throw 'BranchAdmin did not receive its assigned Employee.' }
    $escalationAttempt = @{
        username = $EmployeeA; fullName = 'QA Employee Profile Updated'; role = 'SYSTEM_ADMIN'
        isActive = $false; branchId = $branchIdB; canReceive = $true; canIssue = $true
        employeeCode = $tempEmployee.employeeCode; position = 'QA Profile'
    }
    Assert-Status (Invoke-Api 'PUT' "api/management/users/$employeeId" $Tokens.TEMP_BRANCH_ADMIN $escalationAttempt) 200 'BranchAdmin updates Employee profile in own scope'
    $afterUpdate = Invoke-Api 'GET' 'api/management/users' $Tokens.TEMP_BRANCH_ADMIN
    $updatedEmployee = @($afterUpdate.Body | Where-Object username -eq $EmployeeA) | Select-Object -First 1
    Write-Case 'BranchAdmin cannot change Employee role or active state' 'EMPLOYEE / active' "$($updatedEmployee.role) / $($updatedEmployee.isActive)" ($updatedEmployee.role -eq 'EMPLOYEE' -and $updatedEmployee.isActive -eq $true -and $updatedEmployee.branchId -eq $branchIdA)
    Write-Case 'BranchAdmin cannot reassign Employee through profile update' ([string]$branchIdA) ([string]$updatedEmployee.branchId) ($updatedEmployee.branchId -eq $branchIdA)
    $profileSql = Sql "SELECT CONCAT(r.Code,N'|',CONVERT(varchar(1),u.IsActive),N'|',ep.Position,N'|',COALESCE(CONVERT(varchar(12),ea.BranchId),N'0'),N'|',CONVERT(varchar(1),COALESCE(p.CanReceive,0)),N'|',CONVERT(varchar(1),COALESCE(p.CanIssue,0))) FROM dbo.AppUser u JOIN dbo.Role r ON r.Id=u.RoleId JOIN dbo.EmployeeProfile ep ON ep.UserId=u.Id LEFT JOIN dbo.EmployeeBranchAssignment ea ON ea.EmployeeProfileId=ep.Id AND ea.EffectiveTo IS NULL LEFT JOIN dbo.EmployeeStockPermission p ON p.EmployeeProfileId=ep.Id AND p.BranchId=ea.BranchId WHERE u.Id=$employeeId;"
    $profileExpected = "EMPLOYEE|1|QA Profile|$branchIdA|0|0"
    Write-Case 'BranchAdmin profile update preserves protected SQL fields' $profileExpected $profileSql ($profileSql -eq $profileExpected)

    $existingAdminUsers = Invoke-Api 'GET' 'api/management/users' $Tokens.BRANCH_ADMIN_01
    Assert-Status $existingAdminUsers 200 'BranchAdmin 01 reads Employee list in scope'
    $employeeBVisible = @($existingAdminUsers.Body | Where-Object username -eq 'demo.employee.b').Count -gt 0
    Write-Case 'BranchAdmin 01 cannot see Employee B from Demo 02' 'not visible' ([string]$employeeBVisible) (-not $employeeBVisible)

    $existingAdmin = Invoke-Api 'GET' 'api/management/branches' $Tokens.BRANCH_ADMIN_01
    Assert-Status $existingAdmin 200 'Existing BranchAdmin reads own branch list'
    Write-Case 'Existing BranchAdmin sees only Demo 01' 'BF-DEMO-01' (($existingAdmin.Body | ForEach-Object code) -join ',') (@($existingAdmin.Body).Count -eq 1 -and $existingAdmin.Body[0].code -eq 'BF-DEMO-01')

    Assert-Status (Invoke-Api 'GET' "api/management/branches/$($demo02.id)/stock" $Tokens.BRANCH_ADMIN_01) 403 'BranchAdmin cannot read stock from another branch'
    $branchAdminStockBefore = Get-StockSignature ([int]$demo02.id)
    Assert-Status (Invoke-Api 'POST' 'api/management/stock' $Tokens.BRANCH_ADMIN_01 @{
        branchId = [int]$demo02.id; type = 'IN'; reason = 'QA denied cross-branch write'
        lines = @(@{ ingredientId = 1; quantity = 1; rowVersion = '' })
    }) 403 'BranchAdmin cannot write stock in another branch'
    $branchAdminStockAfter = Get-StockSignature ([int]$demo02.id)
    Write-Case 'BranchAdmin denied stock write leaves SQL unchanged' $branchAdminStockBefore $branchAdminStockAfter ($branchAdminStockBefore -eq $branchAdminStockAfter)
    Assert-Status (Invoke-Api 'PUT' "api/management/branches/$($demo02.id)/availability/products/1" $Tokens.BRANCH_ADMIN_01 @{ isAvailable = $false }) 403 'BranchAdmin cannot change availability outside assigned branch'
    $demoEmployeeBId = [int](Sql "SELECT Id FROM dbo.AppUser WHERE Username=N'demo.employee.b' AND Deleted=0;")
    if ($demoEmployeeBId -le 0) { throw 'Could not find demo.employee.b in BranchFlow_Test.' }
    Assert-Status (Invoke-Api 'PUT' "api/management/users/$demoEmployeeBId" $Tokens.BRANCH_ADMIN_01 @{
        username = 'demo.employee.b'; fullName = 'Must not be updated across branches'; role = 'SYSTEM_ADMIN'
        isActive = $false; branchId = [int]$demo02.id; position = 'Denied'
    }) 403 'BranchAdmin cannot update Employee profile outside assigned branch'
    Assert-Status (Invoke-Api 'POST' 'api/management/branches' $Tokens.BRANCH_ADMIN_01 $forbiddenBranch) 403 'BranchAdmin cannot create branch'
    Assert-Status (Invoke-Api 'POST' 'api/management/users' $Tokens.BRANCH_ADMIN_01 @{
        username = "deny-$RunId"; password = (New-TemporaryPassword); fullName = 'Denied'; role = 'EMPLOYEE'
    }) 403 'BranchAdmin cannot create accounts'
    $productBefore = Get-ProductSignature 1
    Assert-Status (Invoke-Api 'POST' 'api/management/products' $Tokens.BRANCH_ADMIN_01 @{
        name = 'Must not be created'; categoryId = 1; isActive = $true
    }) 403 'BranchAdmin cannot create or change product prices'
    Assert-Status (Invoke-Api 'PUT' 'api/management/products/1' $Tokens.BRANCH_ADMIN_01 @{
        id = 1; name = 'Must not be changed'; categoryId = 1; isActive = $true
    }) 403 'BranchAdmin cannot update a product or its prices'
    $productAfter = Get-ProductSignature 1
    Write-Case 'BranchAdmin denied product/price changes leave SQL unchanged' $productBefore $productAfter ($productBefore -eq $productAfter)
    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.BRANCH_ADMIN_01 @{
        userId = 1; branchId = [int]$demo01.id
    }) 403 'BranchAdmin cannot create assignments'
    Write-Case 'BranchAdmin create-user denial leaves no SQL row' '0' (Sql "SELECT COUNT(*) FROM dbo.AppUser WHERE Username=N'deny-$RunId';") ((Sql "SELECT COUNT(*) FROM dbo.AppUser WHERE Username=N'deny-$RunId';") -eq '0')

    Assert-Status (Invoke-Api 'GET' "api/management/branches/$($demo02.id)/stock" $Tokens.EMPLOYEE_A) 403 'Employee cannot read stock from another branch'
    $employeeStockBefore = Get-StockSignature ([int]$demo02.id)
    Assert-Status (Invoke-Api 'POST' 'api/management/stock' $Tokens.EMPLOYEE_A @{
        branchId = [int]$demo02.id; type = 'IN'; reason = 'QA denied cross-branch write'
        lines = @(@{ ingredientId = 1; quantity = 1; rowVersion = '' })
    }) 403 'Employee cannot write stock in another branch'
    $employeeStockAfter = Get-StockSignature ([int]$demo02.id)
    Write-Case 'Employee denied stock write leaves SQL unchanged' $employeeStockBefore $employeeStockAfter ($employeeStockBefore -eq $employeeStockAfter)
    Assert-Status (Invoke-Api 'GET' "api/management/branches/$($demo01.id)/stock" $Tokens.EMPLOYEE_A) 200 'Employee can read stock within assigned branch'
    Assert-Status (Invoke-Api 'POST' 'api/management/users' $Tokens.EMPLOYEE_A @{
        username = "deny-employee-$RunId"; password = (New-TemporaryPassword); fullName = 'Denied'; role = 'EMPLOYEE'
    }) 403 'Employee cannot create accounts'
    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.EMPLOYEE_A @{
        userId = 1; branchId = [int]$demo01.id
    }) 403 'Employee cannot create assignments'
    Write-Case 'Employee create-user denial leaves no SQL row' '0' (Sql "SELECT COUNT(*) FROM dbo.AppUser WHERE Username=N'deny-employee-$RunId';") ((Sql "SELECT COUNT(*) FROM dbo.AppUser WHERE Username=N'deny-employee-$RunId';") -eq '0')

    $serializedUsers = ConvertTo-Json -InputObject $scopedUsers.Body -Depth 12 -Compress
    Write-Case 'Management response omits password and hash fields' 'no password/hash field' 'not present' ($serializedUsers -notmatch '"(password|passwordHash)"\s*:')

    Write-Output 'Local API/SQL role, assignment, and branch verification completed.'
}
finally {
    foreach ($token in $Tokens.Values) { try { Invoke-Api 'POST' 'api/auth/logout' $token | Out-Null } catch { } }
    try {
        Sql $cleanupSql | Out-Null
        Write-Output 'Temporary test branches, users, profiles and assignments were cleaned.'
    }
    catch {
        Write-Warning 'Automatic cleanup could not be confirmed. Inspect BranchFlow_Test for QA110A/QA110B and qa110.* records before any rerun.'
    }
    $Results | Format-Table -AutoSize | Out-String | Write-Output
    $failed = @($Results | Where-Object Result -eq 'FAIL').Count
    Write-Output "Management API cases recorded: $($Results.Count); failures: $failed."
    $TempPasswords.Clear()
    $Tokens.Clear()
}
