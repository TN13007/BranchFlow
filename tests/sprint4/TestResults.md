# Sprint 4 QA Test Results — SCRUM-89/90/91

Owner: Nguyễn Đặng Hòa Thuận. Updated: 03/11/2026 (Asia/Ho_Chi_Minh). Case source: [TestCases.md](TestCases.md). This file records only observed executions and readiness gates. No Sprint 4 case is marked Pass from source inspection or from a mock test.

## Run 2026-11-03-A: current integrated branch preflight

| Field | Recorded value |
|---|---|
| Runner | Codex assisting Nguyễn Đặng Hòa Thuận |
| Local branch / SHA | `codex/sprint4-qa-plan` / `ca9b56e593278595bcf1de250e222c6f39056439` (source `origin/khang`) |
| SDK | `10.0.403`; repo `global.json` is the one in `origin/khang` |
| DB schema/seed | Repo baseline v1 and `002-reference-data.sql` inspected; no local `BranchFlow_Test` verification run in this session |
| API/Web runtime | Not started for auth/order execution; no demo seed or auth/order API on this SHA |
| `dotnet test tests/BranchFlow.Tests/BranchFlow.Tests.csproj -c Release --logger 'console;verbosity=normal'` | **23 Passed, 0 Failed, 0 Skipped**; 2 menu service and 21 Web cart/session transport tests. Observed console output. |
| `dotnet test tests/BranchFlow.Tests/BranchFlow.Tests.csproj -c Release --no-restore --logger trx` | **23 Passed, 0 Failed, 0 Skipped**. Local TRX: `tests/BranchFlow.Tests/TestResults/Thuan_Nguyen_TYCHE_2026-11-03_22_23_36_net10.0.trx` (generated artifact, not a shared Jira attachment). |
| `dotnet build BranchFlow.slnx -c Release --no-restore` first attempt | Failed `NETSDK1004` because API project assets were not restored in this new worktree. This was a setup error, not a product defect. |
| `dotnet restore BranchFlow.slnx`, then `dotnet build BranchFlow.slnx -c Release --no-restore` | Restore succeeded; build succeeded with **0 warnings, 0 errors**. |

The xUnit run covers `CartTests`, `SessionTransportTests`, and `OrderMenuServiceTests`. In particular, its `PreviewUsesToppingQuantityPerCupAndRequestContainsOnlyContractFields` test asserts 98,000 in Web preview, and `MapsDatabasePriceAndProductSizeIdWithoutChangingContract` uses a mocked repository. These results do **not** prove the server reads current SQL prices, rejects unknown JSON at HTTP, saves an order, rolls back detail/counter, or revokes an API session. `SessionTransportTests` exercises Web transport with fakes; it does not prove browser cookie behavior or live refresh rotation.

## Readiness gates observed on the same SHA

| Gate / related cases | Observation | State and required input |
|---|---|---|
| Auth API (QA-S4-001–018; SCRUM-89) | `src/BranchFlow.Api/Controllers` contains `OrderMenuController` but no auth/branches controller; `Program.cs` registers menu and JWT security only. | **Blocked for runtime execution**. BE1/Thuần must supply login/refresh/logout/me/accessible endpoints, secure seed, and test TTL. FE/Thành must supply integrated browser build for 017–018. Cases remain Not Run. |
| Order service/HTTP/SQL (QA-S4-027–042; SCRUM-90) | No order service/controller in this SHA; test project has no order service tests. `database/002-reference-data.sql` has no user, priced menu or order fixture. | **Blocked for runtime execution**. BE2/Tài must supply service, endpoint, reviewed seed and fault fixture; BE1 auth needed for authenticated calls. Cases remain Not Run. |
| Browser E2E (QA-S4-017–018, 025–026, 043–044; SCRUM-91) | Web screens/client exist, but live auth/create API and full demo seed are absent. | **Blocked for integrated execution**. BE1 + BE2 endpoints/seed and FE integration on known SHAs. Cases remain Not Run. |
| SQL baseline verification (database preflight) | `sqlcmd` was not found in PATH and no `MSSQL*` Windows service was returned on this machine; `Verify-Database.ps1` was not run. | **Not Run**. Provide a local SQL Server instance and `BranchFlow_Test` with scripts 001/002; verify identity before running. The repo's `database/VERIFICATION.md` describes a *previous baseline run*, not a run by this QA session. |

No HTTP response, SQL order row, browser screenshot, or runtime bug has been observed in this run. Missing planned endpoints are dependency gates, not fabricated defects. No token/password/secret was logged.

## Per-case starting record

Every catalog case has an explicit result row. Not Run means no execution of that case has been attempted; the readiness gates above explain why the runtime groups cannot yet proceed. Append actual runs below rather than replacing this starting record.

| Case ID | Actual | Status | Commit | Evidence | Bug | Runner |
|---|---|---|---|---|---|---|
| QA-S4-001 | — | Not Run | — | — | — | — |
| QA-S4-002 | — | Not Run | — | — | — | — |
| QA-S4-003 | — | Not Run | — | — | — | — |
| QA-S4-004 | — | Not Run | — | — | — | — |
| QA-S4-005 | — | Not Run | — | — | — | — |
| QA-S4-006 | — | Not Run | — | — | — | — |
| QA-S4-007 | — | Not Run | — | — | — | — |
| QA-S4-008 | — | Not Run | — | — | — | — |
| QA-S4-009 | — | Not Run | — | — | — | — |
| QA-S4-010 | — | Not Run | — | — | — | — |
| QA-S4-011 | — | Not Run | — | — | — | — |
| QA-S4-012 | — | Not Run | — | — | — | — |
| QA-S4-013 | — | Not Run | — | — | — | — |
| QA-S4-014 | — | Not Run | — | — | — | — |
| QA-S4-015 | — | Not Run | — | — | — | — |
| QA-S4-016 | — | Not Run | — | — | — | — |
| QA-S4-017 | — | Not Run | — | — | — | — |
| QA-S4-018 | — | Not Run | — | — | — | — |
| QA-S4-019 | — | Not Run | — | — | — | — |
| QA-S4-020 | — | Not Run | — | — | — | — |
| QA-S4-021 | — | Not Run | — | — | — | — |
| QA-S4-022 | — | Not Run | — | — | — | — |
| QA-S4-023 | — | Not Run | — | — | — | — |
| QA-S4-024 | — | Not Run | — | — | — | — |
| QA-S4-025 | — | Not Run | — | — | — | — |
| QA-S4-026 | — | Not Run | — | — | — | — |
| QA-S4-027 | — | Not Run | — | — | — | — |
| QA-S4-028 | — | Not Run | — | — | — | — |
| QA-S4-029 | — | Not Run | — | — | — | — |
| QA-S4-030 | — | Not Run | — | — | — | — |
| QA-S4-031 | — | Not Run | — | — | — | — |
| QA-S4-032 | — | Not Run | — | — | — | — |
| QA-S4-033 | — | Not Run | — | — | — | — |
| QA-S4-034 | — | Not Run | — | — | — | — |
| QA-S4-035 | — | Not Run | — | — | — | — |
| QA-S4-036 | — | Not Run | — | — | — | — |
| QA-S4-037 | — | Not Run | — | — | — | — |
| QA-S4-038 | — | Not Run | — | — | — | — |
| QA-S4-039 | — | Not Run | — | — | — | — |
| QA-S4-040 | — | Not Run | — | — | — | — |
| QA-S4-041 | — | Not Run | — | — | — | — |
| QA-S4-042 | — | Not Run | — | — | — | — |
| QA-S4-043 | — | Not Run | — | — | — | — |
| QA-S4-044 | — | Not Run | — | — | — | — |

## Run 2026-11-03-B: local SQL baseline and seed

After run A, SQL Server 2025 Express `17.0.1000.7` was installed as `.\SQLEXPRESS` with Windows Authentication. SSMS `22.11.3` was installed and launched; its installer requested a Windows restart, which was not performed in this run. `BranchFlow_Dev` and `BranchFlow_Test` were confirmed absent, then created with `Vietnamese_100_CI_AI`. `database/001-baseline.sql` and `database/002-reference-data.sql` completed on each database. Each now has 35 tables, 4 roles, 2 sizes, 4 ingredient units, and 0 users; no demo menu/order seed was added. The API's `ConnectionStrings:BranchFlowDb` was set to Dev in local User Secrets without committing its value.

`./database/Verify-Database.ps1 -Server '.\SQLEXPRESS'` ran twice on `BranchFlow_Test` and passed. The second run's [redacted local output](DatabaseSetupResult-2026-11-03.txt) records SQL metadata/constraints, counter 10000 boundary, transaction rollback, 12 distinct concurrent codes, and fixture cleanup. This changes the **SQL baseline readiness gate to Pass**. It does not turn QA-S4-038/039 or any other catalog case into Pass: the script tests DB rules and the counter procedure, not the missing order API/service transaction.

## Next execution log template

Append one row for each attempted case. Preserve earlier runs when retesting. The `Actual` field must quote an observed status/body or SQL/UI fact, never copy the expected text.

| Run ID | Case ID | Local time (Asia/Ho_Chi_Minh) | Runner | FE SHA / API SHA | Schema / seed | Environment | Expected | Actual | Status | Evidence URL/path | Bug URL | Blocker owner/input |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | — | — | — | Not Run | — | — | — |

When the API is ready, use `BranchFlow_Test` and direct HTTPS calls for auth/order cases. For a create request with client `price` or `totalAmount`, record the 400 response **and** before/after SQL order/detail/counter query. For transaction rollback, record the injected detail failure and before/after SQL query. For E2E, record the same order code and amount in UI, API and SQL. Redact all token values and private credentials before attaching evidence.
