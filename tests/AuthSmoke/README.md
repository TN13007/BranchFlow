# Auth SQL/HTTP smoke

This runner checks auth cases against a built API checkout and the local `BranchFlow_Test` database. It creates unique users, branches, assignments, sessions and refresh tokens, then removes only its own fixture in `finally`. Passwords and JWT keys are random for each run and are never printed.

Build the target API in Release, then build this runner from `Thuanaj`:

```powershell
dotnet build tests/AuthSmoke/AuthSmoke.csproj -c Release
dotnet tests/AuthSmoke/bin/Release/net10.0/AuthSmoke.dll 'C:\path\to\target\BranchFlow'
```

The target checkout must include the auth implementation. The runner refuses any connection other than `localhost` / `BranchFlow_Test`; `BRANCHFLOW_AUTH_TEST_DB` can select a local SQL Server instance while keeping that database name. It uses HTTP port 19119 and stops its API child process after every case to isolate the login rate limiter.

The runner covers QA-S4-001–012 and QA-S4-014. It does not cover natural access/session expiry (013), create-order portions of 015–016, or browser cases 017–018. Results on an unmerged feature branch are preliminary; run again on the integrated release candidate before claiming Sprint acceptance.

QA-S4-009 repeats concurrent refresh three times within one catalog case. Each attempt requires one 200, one 401, exactly one descendant token, and a revoked session after reuse. Extra attempt lines do not increase the catalog case count. If the host reports a SQL deadlock, only a redacted diagnostic is printed; full host logs and token values are not exported.
