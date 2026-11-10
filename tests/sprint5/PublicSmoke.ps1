param(
    [Parameter(Mandatory=$true)][string]$ApiBaseUrl,
    [Parameter(Mandatory=$true)][string]$WebBaseUrl,
    [Parameter(Mandatory=$true)][ValidatePattern('^[0-9a-fA-F]{7,40}$')][string]$ExpectedSha,
    [Parameter(Mandatory=$true)][ValidatePattern('^[0-9a-fA-F]{7,40}$')][string]$DeployedSha,
    [string]$OutputRoot = 'artifacts/public-smoke'
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'PowerShell 7+ is required.' }
$repoRoot = git rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0 -or -not $repoRoot) { throw 'Run this script from inside the BranchFlow Git repository.' }
if (-not $env:BRANCHFLOW_DEMO_PASSWORD) { throw 'Set BRANCHFLOW_DEMO_PASSWORD in this process only.' }

foreach ($address in @($ApiBaseUrl,$WebBaseUrl)) {
    $uri = [Uri]$address
    if ($uri.Scheme -ne 'https' -or $uri.IsLoopback) {
        throw 'Public smoke accepts HTTPS non-loopback URLs only.'
    }
}
$ExpectedSha = $ExpectedSha.ToLowerInvariant()
$DeployedSha = $DeployedSha.ToLowerInvariant()
if ($ExpectedSha -ne $DeployedSha) {
    throw "The SHA supplied by the deployment owner does not match the release SHA. Expected $ExpectedSha; supplied $DeployedSha."
}

$ApiBaseUrl = $ApiBaseUrl.TrimEnd('/')
$WebBaseUrl = $WebBaseUrl.TrimEnd('/')
$Tokens = @{}
$Results = [System.Collections.Generic.List[object]]::new()
$Failure = $null

function Write-Case([string]$Name,[string]$Expected,[string]$Actual,[bool]$Passed) {
    $Results.Add([pscustomobject]@{
        case = $Name
        expected = $Expected
        actual = $Actual
        result = if ($Passed) { 'PASS' } else { 'FAIL' }
    })
    if (-not $Passed) { throw "$Name expected $Expected but received $Actual." }
    Write-Output "PASS $Name ($Actual)"
}

function Invoke-Api([string]$Method,[string]$Path,[string]$Token='',$Body=$null) {
    $params = @{
        Uri = "$ApiBaseUrl/$Path"
        Method = $Method
        ErrorAction = 'Stop'
        TimeoutSec = 30
    }
    if ($Token) { $params.Headers = @{ Authorization = "Bearer $Token" } }
    if ($null -ne $Body) {
        $params.ContentType = 'application/json'
        $params.Body = ConvertTo-Json -InputObject $Body -Depth 8 -Compress
    }
    try {
        return [pscustomobject]@{ Status=200; Body=(Invoke-RestMethod @params) }
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) { throw }
        $status = [int]$response.StatusCode
        $raw = ''
        try {
            if ($response.Content) { $raw = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() }
        } catch { }
        $parsed = $raw
        if ($raw) { try { $parsed = ConvertFrom-Json -InputObject $raw } catch { } }
        return [pscustomobject]@{ Status=$status; Body=$parsed }
    }
}

function Assert-Status($Response,[int]$Expected,[string]$Name) {
    Write-Case $Name ([string]$Expected) ([string]$Response.Status) ($Response.Status -eq $Expected)
}

function Login([string]$Username,[string]$Role) {
    $loginBody = @{
        username = $Username
        password = $env:BRANCHFLOW_DEMO_PASSWORD
    }
    $response = Invoke-Api 'POST' 'api/auth/login' '' $loginBody
    if ($response.Status -eq 429) {
        Write-Output "Login rate limit reached for $Role; waiting for the configured one-minute window."
        Start-Sleep -Seconds 60
        $response = Invoke-Api 'POST' 'api/auth/login' '' $loginBody
    }
    Assert-Status $response 200 "Public login $Role"
    if (-not $response.Body.accessToken) { throw "Public login $Role returned no access token." }
    $Tokens[$Role] = [string]$response.Body.accessToken
    $me = Invoke-Api 'GET' 'api/auth/me' $Tokens[$Role]
    Assert-Status $me 200 "Public identity endpoint $Role"
}

try {
    $web = Invoke-WebRequest -Uri "$WebBaseUrl/" -Method Get -MaximumRedirection 5 -TimeoutSec 30
    Write-Case 'Public Web root responds over HTTPS' '200' ([string][int]$web.StatusCode) ([int]$web.StatusCode -eq 200)
    Write-Case 'Public Web root is HTML' 'text/html' ([string]$web.Headers['Content-Type']) ([string]$web.Headers['Content-Type'] -match 'text/html')

    Assert-Status (Invoke-Api 'GET' 'api/management/branches') 401 'Public API rejects request without bearer token'

    Login 'demo.systemadmin' 'SYSTEM_ADMIN'
    Login 'demo.owner' 'OWNER'
    Login 'demo.branchadmin.01' 'BRANCH_ADMIN_01'
    Login 'demo.employee.a' 'EMPLOYEE_A'

    $system = Invoke-Api 'GET' 'api/management/branches' $Tokens.SYSTEM_ADMIN
    Assert-Status $system 200 'Public SystemAdmin branch list'
    $systemCodes = @($system.Body | ForEach-Object code)
    foreach ($code in @('BF-DEMO-01','BF-DEMO-02','BF-DEMO-03')) {
        Write-Case "SystemAdmin sees $code" $code ($systemCodes -join ',') ($systemCodes -contains $code)
    }
    $branch02 = @($system.Body | Where-Object code -eq 'BF-DEMO-02') | Select-Object -First 1
    if (-not $branch02) { throw 'Public API response did not contain BF-DEMO-02.' }

    $owner = Invoke-Api 'GET' 'api/management/branches' $Tokens.OWNER
    Assert-Status $owner 200 'Public Owner branch list'
    $ownerCodes = @($owner.Body | ForEach-Object code)
    $ownerDemoCount = @($ownerCodes | Where-Object { $_ -in @('BF-DEMO-01','BF-DEMO-02','BF-DEMO-03') }).Count
    Write-Case 'Owner can read demo branch data' '3 demo branches' ([string]$ownerDemoCount) ($ownerDemoCount -eq 3)
    Assert-Status (Invoke-Api 'GET' 'api/management/users' $Tokens.OWNER) 403 'Public Owner cannot read management user directory'
    Assert-Status (Invoke-Api 'POST' 'api/management/branches' $Tokens.OWNER @{
        code='PUBLIC-DENY'; name='Must not be created'; address='Smoke test'; isActive=$true
    }) 403 'Public Owner cannot create a branch'

    $branchAdmin = Invoke-Api 'GET' 'api/management/branches' $Tokens.BRANCH_ADMIN_01
    Assert-Status $branchAdmin 200 'Public BranchAdmin branch list'
    Write-Case 'BranchAdmin 01 is restricted to Demo 01' 'BF-DEMO-01 only' (@($branchAdmin.Body | ForEach-Object code) -join ',') (@($branchAdmin.Body).Count -eq 1 -and $branchAdmin.Body[0].code -eq 'BF-DEMO-01')
    Assert-Status (Invoke-Api 'GET' "api/management/branches/$($branch02.id)/stock" $Tokens.BRANCH_ADMIN_01) 403 'Public BranchAdmin cannot read Demo 02 stock'
    Assert-Status (Invoke-Api 'POST' 'api/management/assignments' $Tokens.BRANCH_ADMIN_01 @{userId=1;branchId=[int]$branch02.id}) 403 'Public BranchAdmin cannot create assignments'

    $employeeOwn = Invoke-Api 'GET' 'api/management/branches' $Tokens.EMPLOYEE_A
    Assert-Status $employeeOwn 200 'Public Employee branch list'
    Write-Case 'Employee A is restricted to Demo 01' 'BF-DEMO-01 only' (@($employeeOwn.Body | ForEach-Object code) -join ',') (@($employeeOwn.Body).Count -eq 1 -and $employeeOwn.Body[0].code -eq 'BF-DEMO-01')
    Assert-Status (Invoke-Api 'GET' "api/management/branches/$($branch02.id)/stock" $Tokens.EMPLOYEE_A) 403 'Public Employee cannot read Demo 02 stock'
    Assert-Status (Invoke-Api 'POST' 'api/management/users' $Tokens.EMPLOYEE_A @{
        username='public-deny'; password='not-used'; fullName='Must not be created'; role='EMPLOYEE'
    }) 403 'Public Employee cannot create an account'

    Write-Output 'Public HTTP smoke completed. It did not inspect SQL or perform browser UI acceptance.'
}
catch {
    $Failure = $_.Exception.Message
    throw
}
finally {
    foreach ($token in $Tokens.Values) {
        try { Invoke-Api 'POST' 'api/auth/logout' $token | Out-Null } catch { }
    }

    $out = Join-Path $repoRoot $OutputRoot
    New-Item -ItemType Directory -Force -Path $out | Out-Null
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
    $manifest = [ordered]@{
        checkedAtUtc = [DateTime]::UtcNow.ToString('o')
        expectedReleaseSha = $ExpectedSha
        deployedShaProvidedByReleaseOwner = $DeployedSha
        apiBaseUrl = $ApiBaseUrl
        webBaseUrl = $WebBaseUrl
        results = @($Results)
        overall = if ($Failure) { 'Failed' } else { 'Completed' }
        limitations = @(
            'The deployed SHA is supplied by the deployment owner; this script does not perform remote source attestation.',
            'This is HTTP smoke only, not browser E2E, stakeholder UAT, or SQL verification.'
        )
        failure = $Failure
    }
    $manifest | ConvertTo-Json -Depth 10 | Set-Content -Path (Join-Path $out "public-smoke-$stamp.json") -Encoding utf8
    $Results | Format-Table -AutoSize | Out-String | Write-Output
    Write-Output "Public smoke evidence written to $out."
    $Tokens.Clear()
    Remove-Item Env:BRANCHFLOW_DEMO_PASSWORD -ErrorAction SilentlyContinue
}
