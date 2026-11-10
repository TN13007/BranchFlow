param(
    [ValidateSet('linux-x64')][string]$RuntimeIdentifier = 'linux-x64',
    [string]$OutputRoot = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSVersion.Major -lt 7) { throw 'PowerShell 7+ is required.' }

$root = git rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0 -or -not $root) { throw 'Run this script from inside the BranchFlow Git repository.' }
Push-Location $root
try {
    $workingTree = git status --porcelain
    if ($LASTEXITCODE -ne 0) { throw 'Could not read Git working-tree status.' }
    if ($workingTree) {
        throw 'Release must be built from a clean committed working tree so the manifest SHA identifies the exact published source.'
    }

    $sdk = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0 -or $sdk -notmatch '^10\.') {
        throw "The .NET 10 SDK required by global.json is unavailable. Detected: $sdk"
    }

    $testConnection = $env:BRANCHFLOW_TEST_CONNECTION_STRING
    if (-not $testConnection -or $testConnection -notmatch '(?i)(Database|Initial Catalog)\s*=\s*BranchFlow_Test(?:;|$)') {
        throw 'Set BRANCHFLOW_TEST_CONNECTION_STRING to a local BranchFlow_Test database. Release tests must not silently skip SQL integration tests.'
    }
    if ($testConnection -notmatch '(?i)(Server|Data Source)\s*=\s*(localhost|127\.0\.0\.1|\.)(\\[^;]+)?(?:;|$)') {
        throw 'Release verification accepts only a local SQL Server BranchFlow_Test database.'
    }

    $output = Join-Path $root $OutputRoot
    $staging = Join-Path $output 'staging'
    $apiPublish = Join-Path $staging 'api'
    $webPublish = Join-Path $staging 'web'
    New-Item -ItemType Directory -Force -Path $output | Out-Null
    if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $apiPublish,$webPublish | Out-Null

    function Invoke-DotNet([string[]]$Arguments, [string]$LogName) {
        $log = Join-Path $output $LogName
        if (Test-Path $log) { Remove-Item $log -Force }
        & dotnet @Arguments 2>&1 | Tee-Object -FilePath $log
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) { throw "dotnet $($Arguments -join ' ') failed with exit code $exitCode. See $log." }
    }

    Invoke-DotNet @('restore','BranchFlow.slnx') 'restore.log'
    Invoke-DotNet @('build','BranchFlow.slnx','--configuration','Release','--no-restore') 'build-release.log'
    Invoke-DotNet @('test','BranchFlow.slnx','--configuration','Release','--no-build') 'test-release.log'

    Invoke-DotNet @(
        'publish','src/BranchFlow.Api/BranchFlow.Api.csproj',
        '--configuration','Release','--runtime',$RuntimeIdentifier,
        '--self-contained','false','--output',$apiPublish
    ) 'publish-api.log'
    Invoke-DotNet @(
        'publish','src/BranchFlow.Web/BranchFlow.Web.csproj',
        '--configuration','Release','--runtime',$RuntimeIdentifier,
        '--self-contained','false','--output',$webPublish
    ) 'publish-web.log'

    function Assert-PublishedApp([string]$Directory, [string]$AssemblyName, [bool]$RequireWwwRoot) {
        foreach ($file in @("$AssemblyName.dll","$AssemblyName.deps.json","$AssemblyName.runtimeconfig.json")) {
            if (-not (Test-Path (Join-Path $Directory $file) -PathType Leaf)) {
                throw "Publish output is missing root file: $file"
            }
        }
        if ($RequireWwwRoot) {
            $wwwroot = Join-Path $Directory 'wwwroot'
            if (-not (Test-Path $wwwroot -PathType Container) -or
                -not (Get-ChildItem $wwwroot -File -Recurse | Select-Object -First 1)) {
                throw 'Web publish output must contain a non-empty wwwroot directory.'
            }
        }

        foreach ($settings in Get-ChildItem $Directory -Filter 'appsettings*.json' -File) {
            $json = Get-Content $settings.FullName -Raw | ConvertFrom-Json
            if ($json.ConnectionStrings) {
                foreach ($value in $json.ConnectionStrings.PSObject.Properties.Value) {
                    if (-not [string]::IsNullOrWhiteSpace([string]$value)) {
                        throw "Connection string found in published configuration: $($settings.Name)"
                    }
                }
            }
            if ($json.Jwt.Key -and -not [string]::IsNullOrWhiteSpace([string]$json.Jwt.Key)) {
                throw "JWT signing key found in published configuration: $($settings.Name)"
            }
        }
    }

    Assert-PublishedApp $apiPublish 'BranchFlow.Api' $false
    Assert-PublishedApp $webPublish 'BranchFlow.Web' $true

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $apiZip = Join-Path $output "branchflow-api-$RuntimeIdentifier.zip"
    $webZip = Join-Path $output "branchflow-web-$RuntimeIdentifier.zip"
    foreach ($zip in @($apiZip,$webZip)) { if (Test-Path $zip) { Remove-Item $zip -Force } }

    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $apiPublish,$apiZip,[System.IO.Compression.CompressionLevel]::Optimal,$false)
    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        $webPublish,$webZip,[System.IO.Compression.CompressionLevel]::Optimal,$false)

    function Get-ZipManifest([string]$Path, [string]$AssemblyName, [bool]$RequireWwwRoot) {
        $archive = [System.IO.Compression.ZipFile]::OpenRead($Path)
        try {
            $names = @($archive.Entries | ForEach-Object FullName)
            if ($names | Where-Object { $_.Contains('\') }) { throw "ZIP contains a backslash path: $Path" }
            foreach ($file in @("$AssemblyName.dll","$AssemblyName.deps.json","$AssemblyName.runtimeconfig.json")) {
                if ($names -notcontains $file) { throw "ZIP root does not contain $file" }
            }
            if ($RequireWwwRoot -and -not ($names | Where-Object { $_.StartsWith('wwwroot/',[StringComparison]::Ordinal) })) {
                throw "Web ZIP does not contain wwwroot/: $Path"
            }
            return [ordered]@{
                file = [IO.Path]::GetFileName($Path)
                bytes = (Get-Item $Path).Length
                sha256 = (Get-FileHash $Path -Algorithm SHA256).Hash
                entryCount = $names.Count
                rootFiles = @($names | Where-Object { $_ -notmatch '/' } | Sort-Object)
            }
        }
        finally { $archive.Dispose() }
    }

    $head = (git rev-parse HEAD).Trim()
    $branch = (git branch --show-current).Trim()
    $manifest = [ordered]@{
        product = 'BranchFlow'
        sourceCommit = $head
        sourceBranch = $branch
        builtAtUtc = [DateTime]::UtcNow.ToString('o')
        dotnetSdk = $sdk
        configuration = 'Release'
        runtimeIdentifier = $RuntimeIdentifier
        deploymentModel = 'Framework-dependent; target host must provide .NET 10 ASP.NET Core runtime.'
        verification = [ordered]@{
            restore = 'Passed'
            solutionBuild = 'Passed'
            solutionTests = 'Passed; see test-release.log for exact counts and skipped tests.'
            publicDeployment = 'Not performed by this script.'
        }
        webRuntimeConfig = 'Set Api__BaseUrl to the deployed HTTPS API URL; the checked-in localhost value is for local development.'
        artifacts = @(
            (Get-ZipManifest $apiZip 'BranchFlow.Api' $false),
            (Get-ZipManifest $webZip 'BranchFlow.Web' $true)
        )
    }
    $manifestPath = Join-Path $output 'release-manifest.json'
    $manifest | ConvertTo-Json -Depth 12 | Set-Content -Path $manifestPath -Encoding utf8

    Write-Output "Release packages created in $output"
    Write-Output "API: $apiZip"
    Write-Output "Web: $webZip"
    Write-Output "Manifest: $manifestPath"
    Write-Output 'No deploy, cloud resource, or production database operation was performed.'
}
finally {
    Pop-Location
}
