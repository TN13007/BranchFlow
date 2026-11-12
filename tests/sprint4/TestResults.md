# Sprint 4 QA Test Results — SCRUM-89/90/91

## New run 2026-11-12-R: same-source browser/SQL retest

The [real Web/API and Test DB browser retest](../sprint5/QA-Browser-2026-11-12.md) passed QA-S4-017, 023–026, 043 and 044. Two isolated Employee contexts, cart/SQL comparison, live price and availability changes, and local proxy 401/timeout after upstream commit were checked. The 401 and timeout branches each created exactly one order despite a double-click; Web retained the uncertain cart and directed the user to own orders. A separate QA-S4-018 proxy captured the actual Web-session access token in memory: Web logout forwarded API 204 and direct old-token `/api/auth/me` returned 401. Temporary users and orders were cleaned, and product price/availability restored. Combined with run Q, the current application source has **44/44 formal local catalog cases Pass**. Public smoke, three real participants and PM acceptance remain open.

## New run 2026-11-12-Q: order cases mapped per acceptance condition

The existing OrderSmoke runner was strengthened and rerun against the same integrated application tree `0e0c514`, covering owner/foreign branch denial, missing fields and invalid quantities/IDs, multi-line snapshots, current SQL prices, rollback, twelve concurrent codes/counter, sequence 10000, VN midnight and own-history sorting. Its final output was **152/152 HTTP/SQL assertions Pass**, with fixture cleanup. The [18-case order mapping](../sprint5/QA-Catalog-Order-Mapping-2026-11-12.md) records expected/actual for QA-S4-015, 016 and 027–042 individually. Combined with the 14 Auth and 4 Menu cases below, the **current same-application-source catalog result is 36 Pass / 8 browser cases pending**. This is still not 44/44 or a final public/UAT sign-off.

## New run 2026-11-12-P: current source catalog subset and System Admin UI

On `Thuanaj` `520a7ef` (application tree from integrated main `0e0c514`), AuthSmoke passed all 14 explicitly labelled QA-S4-001–014 cases and MenuSmoke passed the four explicitly labelled QA-S4-019–022 cases. OrderSmoke passed 106 HTTP/SQL assertions and explicitly identified all three QA-S4-041 date-boundary assertions; its broader assertion count is not promoted into 15 further catalog cases without mapping every expected condition. The current same-source **explicit catalog subset is 19 Pass / 25 not yet signed off**. Focused real-browser System Admin product/size/price/topping creation, invalid/duplicate rejection and deactivation also passed on this source; its temporary Test DB fixture was removed. See [12/11 catalog and UI evidence](../sprint5/QA-Catalog-UI-2026-11-12.md). This new run does not alter older candidate snapshots below. Full 44/44 on one candidate and public/UAT acceptance remain open.

Owner: Nguyễn Đặng Hòa Thuận. Updated: 10/11/2026 (Asia/Ho_Chi_Minh). Case source: [TestCases.md](TestCases.md). Tested product core: `Thuanaj` at `19baf09` after merging `main` `bf69ad0` and applying the BE1 fix; QA runners committed separately. Later `main` docs-only `ae8c7f2` was merged as `cd25528`, with clean build and 83/83 SQL-enabled tests. The 06/11 catalog snapshot was 43 Pass / 1 Fail. QA-S4-017, 026 and 044 passed focused browser retests on 10/11. A same-build 44/44 claim still requires a full catalog rerun on the final candidate.

## Run 2026-11-10-N: merged main, management and MVP local retest

Evidence, commands and limits: [10/11 QA record](../sprint5/QA-Retest-2026-11-10.md). `main` was pulled at `bf69ad0` then fast-forward merged into `Thuanaj`; no new branch or conflict. Initial Release build and SQL-enabled suite: 0 warnings/errors, 81/81. Management QA found BF-LOCAL-004 on this core: System Admin could deactivate an assigned Branch Admin (HTTP 200). The existing BE1 fix `1014ac5` was cherry-picked with attribution as `19baf09`; post-fix Release build 0 warnings/errors and SQL-enabled suite **83/83 Pass**, 0 skipped. Management API/SQL matrix then passed **72/72 assertions**, including 400 and unchanged SQL on the deactivation request; temporary management fixtures were cleaned.

| WBS / case | Expected | Observed actual | Result |
|---|---|---|---|
| 3.4.2.5 order lifecycle | Employee creates priced order; Branch Admin edits 1→2 cups and cancels with reason; Employee/cross-branch denial; history and revenue agree with SQL | **27/27** HTTP/SQL checks passed. One 49,000đ order became 98,000đ after edit, then CANCELLED; two adjustment rows persisted; revenue returned to baseline; cancelled order rejected further edit. Test order remains in `BranchFlow_Test`. | Pass for API/SQL; UI edit/cancel still to run |
| 3.5.1.6 / 4.2.4 stock | Import/export, no overdraw, stale RowVersion 409, denied writes have no side effect; sale does not auto-deduct | **13/13** stock HTTP/SQL checks passed; quantity 7→8→7, only two successful transactions persisted; order lifecycle left stock unchanged. Management matrix confirmed out-of-scope stock writes 403 and SQL unchanged. | Pass for HTTP/SQL; UI stock journey still to run |
| 3.2.1.5 / 4.5.5 / 3.1.2.5 management | Four-role scope, branch create/update/duplicate, one effective assignment, protected employee fields and denied SQL writes | Management matrix **72/72 Pass** after BE1 fix; Owner 403 for writes, Branch Admin scoped, duplicate code/assignment 409, inactive branch removed and reactivated, responses omitted password fields. | Pass for API/SQL; UI mutation journey still to run |
| 3.6.2.5 Owner revenue | UI, API, SQL use same date/branch and include completed only | On 10/11 Owner UI showed 128,000đ/2 orders global, Demo 01 98,000đ/1 order, Demo 02 30,000đ/1 order. API returned matching branch rows and SQL totals matched. CANCELLED and NEEDS_REVIEW appeared in order list but not revenue. | Pass for this fixture/date; cross-date status transition still pending |
| QA-S4-026 live menu | Menu reload uses current SQL price; disabled product cannot create an order, cart remains | ProductSize M changed 29,000→31,000đ in Test DB; Web menu and existing cart both showed 31,000đ after reload. Product disabled before submit: Web reported unavailable product, kept 1-cup cart, SQL order count stayed 4. Product/price restored to active/29,000đ. | Pass on `19baf09` |
| QA-S4-017 isolated browser sessions | A and B stay on their own branches; A survives F5/new tab; logout A clears both A tabs without affecting B | Edge Employee A saw BF-DEMO-01 and 4 own orders; independent in-app browser Employee B saw BF-DEMO-02 and 1 own order. A remained logged in after reload and in a new Edge tab. Logout A sent both A tabs to login while B survived reload on BF-DEMO-02. Both sessions were then logged out. Web cookie flags are configured HttpOnly, Secure=Always, SameSite=Lax in `src/BranchFlow.Web/Program.cs`. Temporary Test DB demo passwords were restored exactly from a nine-row SQL backup; the backup table was dropped. | Pass on `19baf09`; full final-candidate catalog still pending |
| QA-S4-044 401/timeout after upstream 201 | Double-submit sends one POST; uncertain result retains cart, blocks resend, and links own order history | Local fault proxy changed one upstream 201 to 401, then withheld one upstream 201 for 30 seconds. Each branch logged exactly one create POST; SQL gained one 29,000đ COMPLETED order (`...-0005` and `...-0006`), own-orders showed each code, and Web retained the 1-cup cart with uncertain-result warning and disabled resend. 401 branch kept the guarded cart after same-tab relogin; its first login POST showed an expired-form message, then full reload/login succeeded. This one-off login error needs a reproducibility check. | Core fault-handling Pass on `19baf09`; login form observation open |
| Order edit/cancel, stock, personnel UI | Manager can adjust/cancel with audit; stock returns to baseline; employee fields remain scoped | Employee A created `...-0007` at 34,000đ with M/Trân châu; Branch Admin changed 1→2 cups to 68,000đ, then canceled with reason. UI showed two history rows; SQL confirmed CANCELLED/68,000đ/two adjustments and completed revenue back at 156,000đ/3 orders. Manager UI stock 7→8→7 with IN #35/OUT #36; overdraw 999 showed an error. Employee A's stock write was denied. Branch Admin edited Employee A's position, saved, then restored the original value; role/branch fields were absent from that editor. | Pass for tested UI paths on `19baf09` |
| Responsive Admin dashboard/orders | 360, 768, 1366 px; navigation and content usable without page overflow | In-app browser actual `innerWidth` 360/768/1366, document scrollWidth 345/753/1351 respectively. Mobile menu opened Orders; order table used an inner horizontal scroll container (604px content in 279px viewport), while page itself did not overflow. Dashboard and orders were visually inspected at these widths. | Pass for inspected screens; other roles/pages still pending |

Fresh AuthSmoke **14/14 catalog cases**, MenuSmoke **34/34 HTTP checks**, OrderSmoke **106/106 checks** all passed and cleaned fixtures. RevenueSmoke initially failed because its hard-coded global total ignored two valid older QA orders in the same Test DB; its baseline assertion was corrected, then **12/12 HTTP checks** passed and cleaned fixtures. This was a test harness issue, not a product revenue defect. The WBS5 demo-data verifier passed after the local named SQL instance was supplied; its four orders and stock 7/15/4 remained intact. Public smoke was not run: no deployed HTTPS URL/SHA was available in the checked handoff.

## Run 2026-11-09-M: merged core retest

`origin/main` at `e83ccb8` was pulled and fast-forward merged into existing `Thuanaj` before this run. Environment: Windows, .NET 10 Release, `localhost\\SQLEXPRESS` / `BranchFlow_Test`, real API/Web over HTTPS localhost. The API, Web and local fault proxy used test-only settings. No production DB or public URL was used. SQL integration tests had skipped initially without `BRANCHFLOW_TEST_CONNECTION_STRING`; the repaired tests resolve demo rows by username/branch code instead of identity IDs. Full solution build: **0 warnings, 0 errors**. Full suite with the Test DB connection: **81 Passed / 0 Failed / 0 Skipped**, twice.

| Scope | Expected | Observed actual | Result |
|---|---|---|---|
| Auth API/SQL | Login, refresh/reuse, expiry, invalid credentials, 401/403, branch scope | AuthSmoke **14/14 catalog cases Pass**; unique fixture cleaned | Pass |
| Menu API/SQL | Current DB prices/states and branch scope | MenuSmoke **34/34 HTTP checks Pass**; fixture cleaned | Pass |
| Create order API/SQL | Valid/invalid orders, scope, transaction and Vietnam business-date boundary | OrderSmoke **106/106 checks Pass**; fixture cleaned | Pass |
| Revenue API/SQL | Owner/System Admin global, Branch Admin scoped, Employee forbidden, completed-only, cancelled excluded, Vietnam date | RevenueSmoke **11/11 HTTP checks Pass** after full solution build; fixture cleaned | Pass |
| Password storage and auth response | Seeded passwords are hashes; login/me expose no password fields | 9/9 seeded users had Identity hash-format strings in Test DB; AuthSmoke response checks passed | Pass for local fixture |
| QA-S4-044, 401 after upstream 201 | One create POST, cart retained across same-tab relogin, uncertain-result guidance, order visible in history | One 29,000đ order `BF-DEMO-01-20261109-0001` (SQL Id 99); cart retained, resend disabled, history showed code; no second POST | Pass on `e83ccb8` |
| QA-S4-044, timeout after upstream 201 | One create POST, cart retained, uncertain-result guidance, order visible in history | One 29,000đ order `BF-DEMO-01-20261109-0002` (SQL Id 100); cart retained, resend disabled, history showed code; no second POST | Pass on `e83ccb8` |
| Four-role local UI spot check | Appropriate navigation/data for Employee, Owner, Branch Admin and System Admin | Employee saw own order/menu; Owner saw all three demo branches and orders without edit controls; Branch Admin saw only Demo 01 and branch menu toggle without common-price editing; System Admin saw all three branches and common catalog editing | Pass for inspected screens only |

The two fault-injection orders remain in `BranchFlow_Test` as local evidence. The prior 43 case passes include browser cases 017 and 026 on an older core; they require a same-build rerun for final catalog sign-off. RevenueSmoke initially returned 404 because the API Release DLL had not been rebuilt after merge; the full solution build corrected the test setup and the rerun passed. This was not recorded as a product defect. Public smoke and stakeholder acceptance remain Not Run.

## Run 2026-11-06-L: independent browser and SQL closeout

Windows Application Control was disabled by the machine owner. A fresh Release solution unit run passed **65/65**. The demo seeder populated the local `BranchFlow_Test` database. The API and Web were run on HTTPS localhost with the test database; Edge and Chrome were separate browser contexts. The local fault proxy [qa-fault-proxy.ps1](qa-fault-proxy.ps1) forwarded requests to the real API and altered only a successful create-order response. This run supersedes the environment block described in run K below.

| Case | Actual observed | Result |
|---|---|---|
| QA-S4-017 | Edge employee A saw BF-DEMO-01; Chrome employee B saw BF-DEMO-02. A survived refresh and a new Edge tab. Logging out A sent both Edge tabs to login while Chrome B remained logged in and scoped to BF-DEMO-02. Cookie security flags are also checked by Web configuration. | Pass |
| QA-S4-026 | Cart initially showed size M at 29,000đ. Changing the test DB price to 31,000đ and reloading the menu updated the preview. Disabling the product caused submit to show “Món hoặc size không còn được bán”, retained the cart and left BF-DEMO-01 order count at zero. Price and product state were restored to 29,000đ/active. | Pass |
| QA-S4-044 | Double-click sent one create POST. The timeout proxy withheld the response after upstream 201; SQL held one 29,000đ order (`BF-DEMO-01-20261106-0001`), while Web kept the cart, showed an uncertain-outcome warning, disabled resend and linked “Đơn của tôi”; that list showed the same order. With a 401 substituted after a second upstream 201, SQL held one more order (`...-0002`) and Web kept the cart in the current view but only showed a login-expired error. It did not show the uncertain-outcome warning; relogin loses the scoped cart. | **Fail** — BF-LOCAL-003 |

The two created orders remain in the local test database as evidence. The demo menu was restored. No production database was used. Case 044 needs a product fix and a complete rerun before Sprint 4 catalog acceptance.

## Run 2026-11-05-K: natural TTL and Vietnam date boundary

The API now accepts a short token TTL or fixed UTC clock only when the environment is `Testing` and the connection targets `localhost/BranchFlow_Test`. Normal lifetimes remain 15 minutes and 7 days. The two smoke runners launched the actual API over HTTP, asserted persisted SQL rows, and cleaned their own fixtures.

| Case | Actual observed | Result | Evidence |
|---|---|---|---|
| QA-S4-013 | Access token naturally expired and `/me` returned 401; refresh before absolute session expiry rotated to a working access token without extending session expiry; separate fixture's `/me` and refresh returned 401 after absolute expiry | Pass | [Auth TTL run](AuthSmokeResult-2026-11-05-Ttl.txt): 14/14 catalog cases, 0 failed, fixture cleanup |
| QA-S4-041 | Real POSTs at controlled 16:59:59Z and 17:00:00Z both returned 201; HTTP and SQL BusinessDate changed to the next Vietnam date at the second instant; SQL CreatedAt equaled the two UTC instants | Pass | [Order boundary run](OrderSmokeResult-2026-11-05-Boundary.txt): 106/106 checks, fixture cleanup |

QA-S4-017, 026 and 044 still need browser execution with two isolated contexts, live menu changes, and a fault proxy after a possible POST commit. Code inspection alone is not counted as a catalog Pass.

Release solution build succeeded with 0 warnings and 0 errors. A fresh 65-test unit run reported 36 Pass and 29 Fail because Windows Application Control blocked loading `BranchFlow.Web.dll` in the test process; a single failing test confirmed `FileLoadException` 0x800711C7 at assembly load. The same policy blocked launching the demo seeder, leaving `BranchFlow_Test` without browser demo accounts/menu. These are environment execution limits, not observed product assertion failures. The earlier 65/65 run on `Thuanaj` is retained as historical evidence; the latest full unit run is not marked green.

## Run 2026-11-05-J: independent automated rerun on Thuanaj

After fast-forwarding `origin/main` into `Thuanaj`, [the rerun record](ThuanajRerun-2026-11-05.txt) confirms Release build 0 warnings/errors, 65/65 unit tests, auth 13/13 catalog cases, menu 34/34 checks and order 101/101 checks on local `BranchFlow_Test`; each smoke runner cleaned its fixture. This independently verifies the automated parts of run I on the branch now pushed for Thuận. The browser/SQL demo evidence below was produced by Khang's agent and was not rerun here. QA-S4-013, 017, 026, 041 and 044 remain Not Run.

## Run 2026-11-05-I: integrated PR 26/27 demo

Case scope and outcomes: [FinalDemoAssessment-2026-11-05.md](FinalDemoAssessment-2026-11-05.md). Order 101 API/SQL checks, auth 13 catalog cases, menu 34 checks; the browser created order BF-DEMO-01-20261105-0002 for 98,000đ and SQL held the same code. 39 Pass / 5 Not Run. Run H and earlier remain below for history.

## Historical integrated review at run H

Run H's source was merged base `2128070` plus review fixes. Its 18 Pass / 26 Not Run count was superseded by runs I–J above. QA-S4-023 was explicitly a cart-unit result at that point.

## Run 2026-11-05-H: integrated review and menu regression retest

Runner: Codex, authorized by Khang. Source: `2128070` plus the review-fix code committed with this record. Local SQL Server `localhost\SQLEXPRESS`, database `BranchFlow_Test`, existing baseline/reference data. Both runners create uniquely identified fixtures and remove their own data in finally; no Dev/cloud seed/reset was run.

### Defects observed before correction

- BF-LOCAL-001 / QA-S4-022: menu smoke passed 28 HTTP checks, then assigned inactive branch returned 403 where 404 was expected. Shared accessible-branch filtering removed inactive/deleted branches before checking assignment. Fixed by using branch scope independent of resource state for menu; active accessible lists and CanAccessBranchAsync retain their filters.
- BF-LOCAL-002 / QA-S4-009: initial integrated auth rerun passed 12/13 cases, but simultaneous refresh returned 200/500. See [failure output](AuthSmokeResult-2026-11-05-BeforeRefreshFix.txt). Follow-up diagnostic identified SQL error 547. Reading/reusing a token can wait for the winning transaction; a timestamp captured before that wait can precede the descendant token's CreatedAt and violate the revoked-time constraint. Refresh now samples time after reading token state, and again after the losing compare-and-update reread. No schema constraints were removed; RowVersion/reuse revocation remain enforced.

### Post-fix verification

| Verification | Observed result | Evidence |
|---|---|---|
| Release solution build | 0 warnings / 0 errors | Agent execution; command `dotnet build BranchFlow.slnx -c Release` |
| Full unit suite | 59 passed, 0 failed, 0 skipped; includes unavailable assigned branch and post-read refresh clock regressions | Agent execution; `dotnet test BranchFlow.slnx -c Release --no-build --no-restore` |
| Menu SQL/HTTP | 34/34 status checks passed; assigned inactive/deleted branch 404, foreign/nonexistent branch 403; fixture cleanup completed | [Menu rerun](MenuSmokeResult-2026-11-05-ReviewFix.txt) |
| Integrated auth SQL/HTTP | 13/13 catalog cases passed; concurrent refresh repeated three times with 200/401, exactly one descendant and revoked winning session after reuse; fixture cleanup completed | [Auth rerun](AuthSmokeResult-2026-11-05-IntegratedReview.txt) |
| Independent confirmation run on the same fixed candidate | Auth 13/13 passed again; three more concurrent-refresh attempts passed, both fixture cleanups completed | [Confirmation output](AuthSmokeResult-2026-11-05-Confirmation.txt) |

Latest integrated case outcomes:

| Cases | Result | Scope / limitation |
|---|---|---|
| QA-S4-001–012, QA-S4-014 | Pass | Real API/SQL auth and branch scope with unique fixtures; no browser acceptance claimed |
| QA-S4-019–022 | Pass | Real API/SQL menu rerun after scope correction |
| QA-S4-023 | Pass (unit scope) | Full suite reruns selected-line preview/edit/remove fixture; no browser claim |
| QA-S4-013, 015–018, 024–044 | Not Run | Natural expiry, remaining order, UI and E2E acceptance are not established; 024 has partial unit coverage only |

Catalog total: **18 Pass / 0 Fail / 0 Blocked / 26 Not Run**, after the recorded successful retests. The earlier failures remain in this log and BugLog; the total describes latest outcomes, not the absence of historical defects. No Jira statuses were changed. Account seeder code is integrated and unit-tested, but this review did not run the interactive seeder or establish priced demo-menu readiness. Sprint Goal and browser/order acceptance remain pending.

---

## Historical executions

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

## Run 2026-11-05-E: main synced into Thuanaj

`Thuanaj` fast-forwarded to `origin/main` at `d66121fc5c17b9b6bc909ce540ca7661ecefb4d5` and was pushed. On this integrated source, the Release solution test run passed **24/24** xUnit tests. After building the API project (0 warnings, 0 errors), the menu SQL/HTTP runner passed **34/34** checks and cleaned its fixtures. The first menu attempt stopped before HTTP checks because the API DLL was absent from this worktree; it was a build setup issue, and the runner cleaned its fixture. Commands and observed results are in [MainSyncResult-2026-11-05.txt](MainSyncResult-2026-11-05.txt).

The latest menu run reconfirms QA-S4-019–022 as Pass. No additional catalog case was completed, so the latest total remains **4 Pass / 40 Not Run**. The integrated source still has no auth or create-order controller. An isolated read-only check of `origin/feat/scrum-73-auth-logout` at `6e603d9` built its API and passed 55/55 xUnit tests; that feature branch is not merged with current `main`, and no direct HTTP auth case was run or marked Pass from it.

## Run 2026-11-05-F: auth feature branch preliminary SQL/HTTP execution

The committed [AuthSmoke runner](../AuthSmoke/README.md) at `dd0708d` launched API source `6e603d9` from `origin/feat/scrum-73-auth-logout` against local `BranchFlow_Test` with unique test-only users, branches, assignments, random passwords and signing key. It restarted the host between cases to isolate the login limiter, observed live HTTP responses and SQL session/refresh state, and deleted its fixtures. The [redacted console log](AuthSmokeResult-2026-11-05.txt) records **13/13 catalog cases Pass, 0 Fail**, exit code 0, and zero remaining test users/branches. No token, password, hash or key value was printed.

| Case ID | Expected and observed actual on feature source `6e603d9` | Feature result | Integrated `Thuanaj` status |
|---|---|---|---|
| QA-S4-001 | Valid login HTTP 200 with pair/UTC expiries and no-store; one SQL session created | Pass | Not Run |
| QA-S4-002 | Wrong password and nonexistent user both HTTP 401 with same generic detail; no session created | Pass | Not Run |
| QA-S4-003 | Inactive locked user HTTP 401 with no-store | Pass | Not Run |
| QA-S4-004 | Missing username/password and whitespace username each HTTP 400 ProblemDetails; no session | Pass | Not Run |
| QA-S4-005 | Sixth invalid login in one minute HTTP 429 with Retry-After 60 and no-store | Pass | Not Run |
| QA-S4-006 | Missing, malformed and expired access JWT each HTTP 401 ProblemDetails with traceId | Pass | Not Run |
| QA-S4-007 | Refresh HTTP 200/no-store rotates pair; new access works at `/api/auth/me`; expiry bounded by original session | Pass | Not Run |
| QA-S4-008 | Old refresh reuse HTTP 401; rotated access then HTTP 401 under reuse revocation policy | Pass | Not Run |
| QA-S4-009 | Two concurrent refresh calls yielded one HTTP 200 and one HTTP 401; SQL session has exactly initial and one descendant token | Pass | Not Run |
| QA-S4-010 | Logout HTTP 204/no-store; old access and refresh HTTP 401; SQL session revoked | Pass | Not Run |
| QA-S4-011 | Logout of session 1 left session 2 `/me` HTTP 200 and refresh HTTP 200 | Pass | Not Run |
| QA-S4-012 | Locking the fixture user after login made `/me` and refresh HTTP 401; fixture restored | Pass | Not Run |
| QA-S4-014 | Owner, System Admin, Branch Admin and Employee `/me` HTTP 200 with DB role; branch lists matched full active scope and omitted inactive/deleted branches | Pass | Not Run |

This run is **preliminary** because the auth feature source is not merged with current `main`/`Thuanaj`. It does not change the integrated candidate's 4 Pass / 40 Not Run count. QA-S4-013 still needs a short-lived TTL fixture and observed natural expiry; QA-S4-015/016 also require create-order actions; QA-S4-017/018 require browser integration. Rerun these 13 cases on the release candidate before recording integrated Pass.

## Run 2026-11-05-G: integrated cart unit acceptance

On `Thuanaj` source `b59a1df`, the full Release xUnit suite passed **26/26**. The filtered [named test output](CartUnitResult-2026-11-05.txt) passed both new cart tests with the `PreviewMenu.Create()` fixture; no SQL or live API was involved.

| Case ID | Expected | Observed actual | Integrated status | Source | Evidence |
|---|---|---|---|---|---|
| QA-S4-023 | Two cups with T1×3 and T2×1 per cup preview 98,000; edit selected line to one cup previews 49,000; other line unchanged; remove selected line | xUnit passed: first line 98,000, selected line 49,000 after edit, second line 38,000 unchanged, cart 87,000, then only second line remained at 38,000 | Pass | `b59a1df` | [Cart unit output](CartUnitResult-2026-11-05.txt) |
| QA-S4-024 | Reject invalid explicit topping/note states and preserve cart; accept topping quantities 1–3; UI decrement 1→0 removes topping from payload | xUnit passed invalid 0/4/duplicate/same-group checks, preserved 98,000 cart, and accepted quantities 1–3. UI decrement behavior was not run | Not Run (unit portion passed) | `b59a1df` | [Cart unit output](CartUnitResult-2026-11-05.txt) |

The latest integrated total is **5 Pass / 39 Not Run**. The preliminary auth feature results in run F remain outside this count.

## Next execution log template

Append one row for each attempted case. Preserve earlier runs when retesting. The `Actual` field must quote an observed status/body or SQL/UI fact, never copy the expected text.

| Run ID | Case ID | Local time (Asia/Ho_Chi_Minh) | Runner | FE SHA / API SHA | Schema / seed | Environment | Expected | Actual | Status | Evidence URL/path | Bug URL | Blocker owner/input |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | — | — | — | Not Run | — | — | — |

When the API is ready, use `BranchFlow_Test` and direct HTTPS calls for auth/order cases. For a create request with client `price` or `totalAmount`, record the 400 response **and** before/after SQL order/detail/counter query. For transaction rollback, record the injected detail failure and before/after SQL query. For E2E, record the same order code and amount in UI, API and SQL. Redact all token values and private credentials before attaching evidence.
