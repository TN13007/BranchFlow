# Sprint 4 QA Test Plan — SCRUM-87

Owner: Nguyễn Đặng Hòa Thuận (QA)

Updated: 03/11/2026 (Asia/Ho_Chi_Minh)

Version under review: `origin/khang` at `ca9b56e593278595bcf1de250e222c6f39056439`

Review: pending Khang; this document does not assert Jira approval.

## Sources and scope

- Jira stories [SCRUM-50](https://khangba153.atlassian.net/browse/SCRUM-50), [SCRUM-51](https://khangba153.atlassian.net/browse/SCRUM-51), [SCRUM-52](https://khangba153.atlassian.net/browse/SCRUM-52); QA tasks [SCRUM-87](https://khangba153.atlassian.net/browse/SCRUM-87) and [SCRUM-88](https://khangba153.atlassian.net/browse/SCRUM-88).
- `GiaoViec_Thuan_QA_v2.zip`: common context, API contract v1, local setup, schema-v1 and Thuận QA assignment. Repo `database/README.md`, `database/VERIFICATION.md`, `database/schema-v1.json`, `src/BranchFlow.Api/Program.cs`, and current test source were checked against the actual branch.
- In scope: login/refresh/logout and session revocation; role and effective branch access; branch menu and cart; create order with server price, snapshots, counter and transaction; own-order list/detail; Web → API → SQL Server flow.
- Out of scope for this Sprint slice: stock, payroll, transfer, management CRUD, cloud deployment, device management, and the whole MVP acceptance claim.

The contract is a target, not proof that an endpoint exists. On the inspected commit the repo has SQL baseline, Web UI/client code, and `GET /api/order-menu`; it has no auth controller or order creation controller. Reference seed has no demo user or priced menu. The 23 existing xUnit tests cover Web state/transport and menu service behavior; they do not prove auth API, order persistence, or browser E2E.

## Environments and entry conditions

| Layer | Environment and tool | Entry condition | Evidence |
|---|---|---|---|
| Unit | .NET 10, xUnit/Moq; `dotnet test tests/BranchFlow.Tests/BranchFlow.Tests.csproj -c Release --logger trx` | Compatible service/DTO interface merged; deterministic fixture independent of API response | TRX and test name, SHA, expected/actual |
| HTTP | Local HTTPS API `https://localhost:7107`, direct HTTP client | Auth/menu/order endpoints available, configured JWT and `BranchFlow_Test`, demo seed, server build SHA | Redacted status/body/headers, request shape, SHA |
| SQL | Local SQL Server `BranchFlow_Test`, `sqlcmd`, `database/Verify-Database.ps1` | Dedicated test DB with schema v1 + reference data + reviewed demo seed; confirm database name before every mutating test | Before/after query, transaction/constraint output, script version |
| Browser | Local Web `https://localhost:7132`, two isolated browser contexts | FE and API integrated on known SHAs; employee/admin/owner users available | Screenshots/video with case ID and same order code as SQL |

Never run destructive fixtures against shared cloud or `BranchFlow_Dev`. Baseline verification tests SQL metadata/constraints and a counter procedure, not order service behavior. Do not reset an existing DB to make a test pass. Keep token values, passwords, password hashes, signing keys, and connection strings out of evidence.

### Required fixtures from BE1/BE2

| Fixture | Owner | Use |
|---|---|---|
| Active Employee A at branch A, Employee B at branch B, Owner, System Admin, Branch Admin, locked user; repeatable seed with private passwords | BE1/Thuần | auth, roles, isolation, second session |
| Branch A/B, active/inactive product and size, branch-disabled product/topping, two topping types, two options in one note group | BE2/Tài | menu, validation, availability |
| Priced `ProductSizeId` and topping IDs read from seed/menu; test price 30,000 + toppings 5,000 and 4,000 if that fixture is explicitly installed | BE2/Tài | independent 98,000 calculation |
| Dedicated way to induce order-detail failure inside transaction; no production fault injection | BE2/Tài with QA | rollback proof |
| Test configuration for short access lifetime and absolute session expiry | BE1/Thuần | expiry without changing system clock |

IDs `101`, `5`, `6`, `7` in examples are illustrative until the seed is confirmed. Record actual IDs and source seed revision before running.

## Exit and reporting

Every in-scope acceptance criterion must map to a case in [TestCases.md](TestCases.md) with an actual result and evidence on the release candidate. A case is **Pass** only after its expected result is observed on the stated build. Differences are **Fail** with a linked bug. Missing build, endpoint, test DB, or fixture is **Blocked** with an owner and needed input; unattempted cases are **Not Run**. A deliberate omission is **Skipped** only with a recorded reason. Blocking bugs are fixed and retested, or Khang explicitly records the acceptance decision. No percentage threshold substitutes for this review.

Bug report: case ID, story/FR, build SHA and DB schema/seed version, environment, sanitized request/steps, expected, actual, impact, severity, priority, owner, evidence and retest link. Severity reflects impact (blocker/major/minor); priority is triage urgency set with PM. Keep Bug Log and Test Results keyed by the same case ID. For each fixed bug, rerun its original case plus relevant regression cases on the fix SHA.

## Traceability matrix

Jira's FR ranges are used as written; the stories do not give an unambiguous one-to-one mapping for every individual FR. The Story/FR association below is therefore at range level until the functional specification is reconciled by Khang.

| Source / acceptance area | Case IDs | Layer | Evidence and result owner |
|---|---|---|---|
| SCRUM-50 / FR-01–04: login, token, locked account | QA-S4-001–006 | HTTP | Auth Test Results / QA; BE1 provides seed |
| SCRUM-50 / FR-01–04: rotation, reuse, logout, expiry | QA-S4-007–013 | HTTP, SQL | Auth Test Results / QA; BE1 provides endpoint and TTL |
| SCRUM-50 / FR-01–04: role, branch, browser isolation | QA-S4-014–018 | HTTP, browser | Auth/E2E Test Results / QA; BE1 + FE |
| SCRUM-51 / FR-13, FR-15–17 (FR-09–12 data dependency): menu availability and cart | QA-S4-019–026 | HTTP, unit, browser, SQL | Menu/E2E Test Results / QA; BE2 + FE |
| SCRUM-52 / FR-14–18 (FR-13 dependency): request validation, server totals, snapshot | QA-S4-027–037 | HTTP, unit, SQL | Order Test Results / QA; BE2 |
| SCRUM-52 / FR-14–18: atomicity, concurrency, date/code, own history | QA-S4-038–044 | HTTP, SQL, browser | Order/E2E Test Results / QA; BE2 + FE |

Cases 001–044 start **Not Run** in the case catalog. The existing 23 passing tests at the inspected commit are a separate baseline observation and do not change those case statuses.

## Execution order and current gate

1. Confirm seed, schema version, SHAs, test DB identity and build; run `Verify-Database.ps1` only on `BranchFlow_Test`.
2. Run auth HTTP cases, then branch/menu, then order validation/price/persistence, then browser E2E.
3. Retest bugs on fix SHAs and run login → menu → create → detail → logout smoke on the intended demo build.
4. Reconcile counts in Test Results and Test Summary; send Khang the evidence links and unresolved risks for review.

At `ca9b56e`, runtime auth/order cases are gated by missing API endpoints and demo seed. The plan and independent expected cases can be reviewed now; runtime acceptance cannot be claimed yet.
