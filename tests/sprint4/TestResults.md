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

## Run 2026-11-03-C: menu API with local SQL fixture

Traceability correction 04/11/2026: the originally recorded local test SHA `c0f5ed58077f12244c4187a8057afda405ea3315` cannot be resolved from the fetched repository. The published menu test source is [cc4d432](https://github.com/TN13007/BranchFlow/commit/cc4d432); the published SQL setup record is [1fb7ea6](https://github.com/TN13007/BranchFlow/commit/1fb7ea6). The original SHA fields below are retained as historical records, not asserted to be reachable or byte-identical to published commits. A new run on the review fix is recorded separately after verification.

The existing `tests/MenuSmoke` runner was expanded in test commit `c0f5ed58077f12244c4187a8057afda405ea3315` and run against `BranchFlow_Test` and the API source at `ca9b56e593278595bcf1de250e222c6f39056439`. It creates uniquely named branch/user/menu/session fixtures, generates test-only password hashes and JWT keys in memory, exercises the real HTTP host and SQL repository, then deletes its own fixtures. The [console evidence](MenuSmokeResult-2026-11-03.txt) shows **34 HTTP status checks**, all Pass, followed by zero remaining branch/product/session fixtures. These 34 checks complete four catalog cases; they are not 34 separate catalog case results.

| Case ID | Expected | Observed actual | Status | Test source commit | Evidence | Bug | Runner |
|---|---|---|---|---|---|---|---|
| QA-S4-019 | Authorized A menu contains only valid branch items and ProductSizeId/current DB price | HTTP 200; branch matched fixture; ProductSizeId and 30,000.50 price matched SQL; topping maxQuantity 3 and note maxSelections 1 | Pass | `c0f5ed5` | [MenuSmoke output](MenuSmokeResult-2026-11-03.txt) | — | Codex assisting Thuận |
| QA-S4-020 | Inactive/deleted/unavailable menu elements omitted; empty collections are arrays | HTTP 200 after each controlled toggle of category/product/product size/size/topping group/topping/note group/option and branch availability; affected collection length 0 | Pass | `c0f5ed5` | [MenuSmoke output](MenuSmokeResult-2026-11-03.txt) | — | Codex assisting Thuận |
| QA-S4-021 | Missing, zero, negative and malformed branchId return 400 ProblemDetails | Four HTTP 400 responses, each with problem+json status and traceId | Pass | `c0f5ed5` | [MenuSmoke output](MenuSmokeResult-2026-11-03.txt) | — | Codex assisting Thuận |
| QA-S4-022 | Out-of-scope/nonexistent branch 403; assigned inactive branch 404 | HTTP 403 for both out-of-scope and nonexistent branch; HTTP 404 for assigned branch made inactive/deleted | Pass | `c0f5ed5` | [MenuSmoke output](MenuSmokeResult-2026-11-03.txt) | — | Codex assisting Thuận |

The same runner also observed missing/malformed/expired/revoked JWT as 401, Owner role and wrong branch as 403, and expired/future branch assignment as 403. These are partial evidence for SCRUM-89. QA-S4-015/016 still include create-order actions, so they remain Not Run until the order endpoint exists. Login/refresh/logout and browser session behavior were not exercised. Run A's SQL-readiness row is historical; Run B supersedes it.

## Run 2026-11-04-D: independent review rerun

Runner: Codex assisting Khang, following delegated review of Thuận's QA package. Exact test/API source: [4b0484d2a40ee5d5588fbd172980535248e2c6ab](https://github.com/TN13007/BranchFlow/commit/4b0484d2a40ee5d5588fbd172980535248e2c6ab). Environment: localhost\SQLEXPRESS / BranchFlow_Test, existing schema v1/reference seed plus unique per-run fixture. No auth/order endpoint or browser E2E is inferred from this run.

Build `BranchFlow.slnx -c Release`: 0 warnings, 0 errors. `dotnet test BranchFlow.slnx -c Release --no-build --no-restore`: 23 Passed, 0 Failed, 0 Skipped. The menu runner passed all 34 HTTP checks and cleaned its fixtures. A second active note group and option are created as a control; every 200 response must preserve that unrelated group, while negative note checks target only the fixture group's ID.

| Case ID | Expected | Actual | Status | Evidence |
|---|---|---|---|---|
| QA-S4-019 | Menu uses permitted branch, ProductSizeId, DB price, topping max 3 and note max 1 | HTTP 200; fixture branch/ID matched; price 30000.50, topping max 3 and fixture note max 1 matched; control note group retained | Pass | [04/11 console log](MenuSmokeResult-2026-11-04.txt) |
| QA-S4-020 | Inactive/deleted/unavailable fixture entries disappear without hiding unrelated note groups | Each controlled negative HTTP 200 check passed; tested note group absent by ID, control option remained visible; cleanup completed | Pass | [04/11 console log](MenuSmokeResult-2026-11-04.txt) |
| QA-S4-021 | Missing/zero/negative/malformed branchId gives 400 ProblemDetails | Four HTTP 400 checks passed with problem+json, status and traceId | Pass | [04/11 console log](MenuSmokeResult-2026-11-04.txt) |
| QA-S4-022 | Foreign/nonexistent branch gives 403; assigned inactive branch gives 404 | Both 403 checks and assigned inactive/deleted branch 404 checks passed | Pass | [04/11 console log](MenuSmokeResult-2026-11-04.txt) |

QA-S4-024's expected wording now distinguishes a UI decrement to zero (remove topping from payload) from an explicit invalid quantity-zero entry. The catalog case remains Not Run; editing expected text and passing existing unit tests does not complete its browser execution. Latest catalog totals remain 4 Pass and 40 Not Run.

## Next execution log template

Append one row for each attempted case. Preserve earlier runs when retesting. The `Actual` field must quote an observed status/body or SQL/UI fact, never copy the expected text.

| Run ID | Case ID | Local time (Asia/Ho_Chi_Minh) | Runner | FE SHA / API SHA | Schema / seed | Environment | Expected | Actual | Status | Evidence URL/path | Bug URL | Blocker owner/input |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | — | — | — | Not Run | — | — | — |

When the API is ready, use `BranchFlow_Test` and direct HTTPS calls for auth/order cases. For a create request with client `price` or `totalAmount`, record the 400 response **and** before/after SQL order/detail/counter query. For transaction rollback, record the injected detail failure and before/after SQL query. For E2E, record the same order code and amount in UI, API and SQL. Redact all token values and private credentials before attaching evidence.
