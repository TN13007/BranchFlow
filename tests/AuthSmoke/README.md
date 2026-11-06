# Auth SQL/HTTP smoke

This runner checks auth cases against a built API checkout and the local `BranchFlow_Test` database. It creates unique users, branches, assignments, sessions and refresh tokens, then removes only its own fixture in `finally`. Passwords and JWT keys are random for each run and are never printed.

Build the target API in Release, then build this runner from `Thuanaj`:

```powershell
dotnet build tests/AuthSmoke/AuthSmoke.csproj -c Release
dotnet tests/AuthSmoke/bin/Release/net10.0/AuthSmoke.dll 'C:\path\to\target\BranchFlow'
```

The target checkout must include the auth implementation. The runner refuses any connection other than `localhost` / `BranchFlow_Test`; `BRANCHFLOW_AUTH_TEST_DB` can select a local SQL Server instance while keeping that database name. It uses HTTP port 19119 and stops its API child process after every case to isolate the login rate limiter.

The runner covers QA-S4-001–014. QA-S4-013 restarts the API in `Testing` with a 5-second access TTL and 25-second absolute session TTL, waits for natural expiry, and checks refresh before and after the absolute limit. These overrides are rejected unless the API connects to local `BranchFlow_Test`. Create-order portions of 015–016 and browser cases 017–018 require their separate integrated checks.

QA-S4-009 repeats concurrent refresh three times within one catalog case. Each attempt requires one 200, one 401, exactly one descendant token, and a revoked session after reuse. Extra attempt lines do not increase the catalog case count. If the host reports a SQL deadlock, only a redacted diagnostic is printed; full host logs and token values are not exported.
