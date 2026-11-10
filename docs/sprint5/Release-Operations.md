# Sprint 5 — Release package handoff (SCRUM-113)

## Purpose and boundary

This handoff prepares separate framework-dependent Linux Release packages for the API and Blazor Web application. It does **not** deploy either application and does not create cloud resources.

The API and Web are published independently because they are separate ASP.NET Core applications. Each archive contains the application files at the archive root. The Web archive must also contain `wwwroot/`.

## Build, test, publish, and package

Run from the repository root on Windows PowerShell 7+ with Git and the .NET 10 SDK selected by `global.json`.

The SQL integration suite is skipped when `BRANCHFLOW_TEST_CONNECTION_STRING` is absent. The packaging script deliberately refuses to proceed unless that variable identifies a local `BranchFlow_Test` database.

```powershell
# Supply the value from your local secret store / existing shell configuration.
# Do not echo it, paste it into chat, or commit it.
$env:BRANCHFLOW_TEST_CONNECTION_STRING = '<local SQL Server connection to BranchFlow_Test>'

./tools/Publish-Release.ps1 -RuntimeIdentifier linux-x64

Remove-Item Env:BRANCHFLOW_TEST_CONNECTION_STRING
```

The script performs, in order:

1. `dotnet restore BranchFlow.slnx`.
2. `dotnet build BranchFlow.slnx --configuration Release --no-restore`.
3. `dotnet test BranchFlow.slnx --configuration Release --no-build`.
4. Publish `BranchFlow.Api` and `BranchFlow.Web` separately for `linux-x64`, framework-dependent.
5. Check root-level DLL, `.deps.json`, and `.runtimeconfig.json` entries.
6. Check the Web archive has a non-empty `wwwroot/`, and ZIP paths use forward slashes.
7. Check published appsettings files do not contain a configured database connection string or JWT signing key.
8. Write SHA-256 hashes, file sizes, source SHA, runtime, build/test status, and archive entries into `release-manifest.json`.

Output:

- `artifacts/release/branchflow-api-linux-x64.zip`
- `artifacts/release/branchflow-web-linux-x64.zip`
- `artifacts/release/release-manifest.json`
- Build, test, restore, and publish logs in `artifacts/release/`

A successful script run is the evidence for SCRUM-113. A source review or a checked-in manifest template is not a successful build.

## Runtime configuration to provide outside the archives

The API reads configuration from the environment at runtime. Khang / the deployment owner must supply values through the hosting platform's secret/configuration facility:

| API setting | Purpose |
|---|---|
| `ConnectionStrings__BranchFlowDb` | Connection to the approved target SQL Server database |
| `Jwt__Key` | JWT signing key, at least 32 bytes |
| `Jwt__Issuer` | Expected token issuer |
| `Jwt__Audience` | Expected token audience |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

The Web application must be configured with:

| Web setting | Purpose |
|---|---|
| `Api__BaseUrl` | Absolute HTTPS URL of the deployed API, ending in `/` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

The checked-in Web `appsettings.json` uses `https://localhost:7107/` for local development. It must be overridden with the real API URL in the deployed environment. Do not write the public URL into a release archive until Khang confirms the actual service endpoint.

Because the packages are framework-dependent, the Linux host must have the .NET 10 ASP.NET Core runtime installed. TLS termination, DNS, firewall, service supervision, database firewall access, and approved hosting plan remain deployment-owner responsibilities.

## Release acceptance checklist

| Check | Evidence required | Current state |
|---|---|---|
| Source SHA selected | Exact Git SHA and branch in manifest | Pending script execution |
| Full Release build | Build log and exit code 0 | Not Run in this workspace |
| SQL-enabled test suite | Test log, exact passed/failed/skipped counts | Not Run in this workspace |
| API publish | Root DLL/deps/runtimeconfig in ZIP | Pending script execution |
| Web publish | Root DLL/deps/runtimeconfig and non-empty wwwroot | Pending script execution |
| Linux archive paths | No backslash entries | Pending script execution |
| Secret configuration | No DB connection or JWT signing key in published appsettings | Pending script execution |
| Manifest | SHA-256 and file sizes for both ZIPs | Pending script execution |
| Deployment | URL and deployed SHA supplied by Khang | Not part of SCRUM-113 |

Do not mark SCRUM-113 Done until the script has actually run, the output has been inspected, and Khang has received the two archives, logs, and manifest.
