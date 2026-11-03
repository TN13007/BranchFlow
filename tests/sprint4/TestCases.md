# Sprint 4 QA cases — SCRUM-88

Owner: Nguyễn Đặng Hòa Thuận. Updated: 03/11/2026. Source: [TestPlan.md](TestPlan.md), Jira SCRUM-50/51/52 and API contract v1 in `GiaoViec_Thuan_QA_v2.zip`.

**Execution record rule:** every row has `Actual = —`, `Status = Not Run`, `Commit = —`, `Evidence = —`, `Bug = —`, and `Runner = —` until an execution is recorded in `TestResults.md` with the same ID. These are explicit initial values for *each* case, not an inferred Pass. Mark Blocked only when an attempted run lacks a prerequisite, and name the owner/input in Test Results. All example IDs are placeholders; replace them with real seed IDs before running. HTTP errors are `application/problem+json` with status/title/detail/traceId and validation `errors`; no HTML redirect or stack/secret.

Fixture codes: **A** active Employee A assigned to branch A; **B** active Employee B assigned to branch B; **O** Owner; **SA** System Admin; **BA** Branch Admin; **L** locked account. BE1 supplies repeatable private credentials and active session seed. BE2 supplies active product/size/toppings/notes at A, disabled variants, and actual IDs. Use only `BranchFlow_Test` for mutation/SQL tests. `S=101`, `T1=5`, `T2=6`, `N=7` below are illustrative and must be resolved from the seed/menu. A priced fixture for calculation has size 30,000, T1 5,000, T2 4,000; if seed differs, calculate the expected amount from the reviewed seed *before* making the request.

| ID | Story / FR | Layer | Preconditions and input | Steps | Expected |
|---|---|---|---|---|---|
| QA-S4-001 | 50 / 01–04 | HTTP | A active; credentials private | POST `/api/auth/login` with valid username/password; inspect response | 200; access/refresh tokens and two UTC expiry values; no password/hash/key; `Cache-Control: no-store` |
| QA-S4-002 | 50 / 01–04 | HTTP | A active | POST login with wrong password, then nonexistent username | Both 401 with same generic auth message; no token or account existence leak |
| QA-S4-003 | 50 / 01–04 | HTTP | L locked | POST login using L's otherwise valid credentials | 401 generic message, no token |
| QA-S4-004 | 50 / 01–04 | HTTP | API running | POST login missing username, missing password, then whitespace username | 400 validation; no token or session created |
| QA-S4-005 | 50 / 01–04 | HTTP | Login limiter configured | Send 6 invalid logins from same test IP within 1 minute | Limit response 429 with `Retry-After`; verify effective threshold against configured policy |
| QA-S4-006 | 50 / 01–04 | HTTP | Protected route exists | GET `/api/auth/me` with no JWT, malformed JWT, then expired JWT | 401 JSON ProblemDetails for each; no HTML redirect or protected data |
| QA-S4-007 | 50 / 01–04 | HTTP | Fresh A token pair | POST `/api/auth/refresh` with refresh token; then GET me with new access | 200 new pair and UTC expiries within absolute session limit; new access works; `no-store` |
| QA-S4-008 | 50 / 01–04 | HTTP/SQL | QA-S4-007 done | POST refresh again with old refresh token; inspect session/token rows without exposing hashes | 401 generic; old refresh cannot be reused; session revocation follows reuse policy |
| QA-S4-009 | 50 / 01–04 | HTTP/SQL | Fresh A pair | Send two simultaneous refresh requests with same refresh token; inspect DB | Exactly one 200 rotation; other 401; no two valid descendants or duplicate session |
| QA-S4-010 | 50 / 01–04 | HTTP | Fresh A access + refresh | POST `/api/auth/logout` with access; GET me using old access; POST refresh using old refresh | 204 then 401 and 401; server session and refresh revoked, not merely Web cookie removed |
| QA-S4-011 | 50 / 01–04 | HTTP | Two independent A sessions | Logout session 1, call me and refresh on session 2 | Session 1 invalid; session 2 remains valid |
| QA-S4-012 | 50 / 01–04 | HTTP/SQL | A active session; BE1 test fixture | Lock A in Test DB while access remains unexpired; call me and refresh | Both 401; no accepted token from locked account; revert only fixture-owned change |
| QA-S4-013 | 50 / 01–04 | HTTP | Short-lived test-only TTL | Wait past access TTL and absolute session TTL in separate fixtures; call me/refresh | Expired access 401; refresh can rotate before absolute expiry only; after absolute expiry 401 |
| QA-S4-014 | 50 / 01–04 | HTTP | O, SA, BA, A active | GET me and accessible branches for each | 200 role matches DB; branch list contains only currently accessible branches; no hash fields |
| QA-S4-015 | 50 / 01–04 | HTTP | O and A tokens, menu at A | O calls menu/create; A calls allowed menu | O receives 403 for employee-only actions; A menu 200 |
| QA-S4-016 | 50 / 01–04 | HTTP | A at branch A; B at branch B | A calls menu and create with branch B ID | 403; no order or counter change at B |
| QA-S4-017 | 50 / 01–04 | Browser | Integrated Web and API; two isolated contexts | Login A and B in separate browser contexts; F5/new tab; logout A | No cross-user token/data; own session survives F5/new tab; B unaffected; cookie HttpOnly/Secure/SameSite=Lax |
| QA-S4-018 | 50 / 01–04 | Browser/HTTP | Integrated Web and API | Logout via Web, then call me directly with captured *redacted in evidence* prelogout access | Web returns to login; old API access 401; no refresh in localStorage/cookie ticket |
| QA-S4-019 | 51 / 13,15–17 | HTTP | A and branch A menu seed | GET `/api/order-menu?branchId=A` | 200 branch A; only active, not-deleted, branch-available categories/products/sizes/toppings/notes; ProductSizes[].id is ProductSizeId |
| QA-S4-020 | 51 / 13,15–17 | HTTP | Branch A menu seed with disabled/deleted variants | Disable product globally, size, category, branch product and topping one at a time in Test fixture; GET menu | Disabled/deleted/unavailable entries omitted; empty category omitted; collections `[]`, not null |
| QA-S4-021 | 51 / 13,15–17 | HTTP | A token | GET menu with missing, zero, negative, malformed branchId | 400 ProblemDetails for invalid query |
| QA-S4-022 | 51 / 13,15–17 | HTTP | A token; nonexistent branch ID and an assigned but inactive Test branch | GET menu for each | Nonexistent/out-of-scope branch 403 without data leak; assigned inactive branch 404 |
| QA-S4-023 | 51 / 15–17 | Unit/browser | Menu fixture with S,T1,T2,N | Add 2 cups S with T1×3 and T2×1 per cup, N; edit cups to 1 and remove line | Preview 98,000 initially from fixture; then 49,000; cart changes only selected line |
| QA-S4-024 | 51 / 15–17 | Unit/browser | Same menu fixture | Add with no size, 0 cups, T1 quantities 0 and 4, duplicate T1, or two notes in one group | Each rejected before submit; cart stays unchanged; topping 1/2/3 accepted |
| QA-S4-025 | 51 / 15–17 | Browser/SQL | A and empty order table | Add/edit/remove cart lines; query SalesOrder/OrderItem before and after | No order/header/detail/counter created until confirmation |
| QA-S4-026 | 51 / 13,15–17 | Browser | Integrated Web/API, A | Open menu, then disable selected product or change price in Test DB; refresh/submit UI | Disabled item cannot submit; price preview refreshes; cart preserved for correction |
| QA-S4-027 | 52 / 14–18 | HTTP/SQL | A and priced fixture | POST create with S, 2 cups, T1×3, T2×1, N | 201; total 98,000 independently calculated as `2 × (30,000 + 3×5,000 + 1×4,000)`; `COMPLETED`; Location GET reads same order |
| QA-S4-028 | 52 / 14–18 | HTTP/SQL | A; two priced sizes | POST two item lines with distinct configs, e.g. line 1 QA-S4-027 plus 1 cup at size price P and no toppings | 201; total `98,000 + P`; quantity 3; detail preserves both lines and snapshots |
| QA-S4-029 | 52 / 14–18 | HTTP/SQL | A | POST `items:[]`, null items, null element or null toppings/notes | 400; no SalesOrder/OrderItem/counter change |
| QA-S4-030 | 52 / 14–18 | HTTP/SQL | A | POST quantity 0, negative, decimal/string; nonpositive IDs | 400; no order or counter change |
| QA-S4-031 | 52 / 14–18 | HTTP/SQL | A | POST nonexistent ProductSizeId, ProductId/SizeId instead of ProductSizeId | 400 for invalid contract/ID; no order |
| QA-S4-032 | 52 / 14–18 | HTTP/SQL | A | POST T1 quantity 1, 2, 3 separately; then 0, 4, duplicate T1 | Valid quantities 201 with matching server totals; invalid quantities/duplicate 400 and no order |
| QA-S4-033 | 52 / 14–18 | HTTP/SQL | A | POST duplicate note ID, two options in same NoteGroup, then empty notes | Duplicates/group conflict 400 and no order; empty notes allowed |
| QA-S4-034 | 52 / 14–18 | HTTP/SQL | A | Add `price`, `totalAmount`, `createdByUserId`, `status` at root separately | Each request 400 before persistence; server does not silently ignore client metadata |
| QA-S4-035 | 52 / 14–18 | HTTP/SQL | A | Add `unitPrice`/`subtotalAmount` to item and `price` to topping separately | Each request 400 before persistence; nested unknown fields rejected |
| QA-S4-036 | 52 / 14–18 | HTTP/SQL | A; menu captured before mutation | Change ProductSize/Topping price in Test DB, POST original IDs without price | Total and snapshots use current DB prices, never stale menu/client price |
| QA-S4-037 | 52 / 14–18 | HTTP/SQL | Existing completed order | Change master product/topping name or price in Test DB; GET detail | Order item/topping/note names and prices remain snapshot values; total unchanged |
| QA-S4-038 | 52 / 14–18 | SQL/HTTP | Dedicated Test DB; BE2 fault fixture | Record order/detail/counter counts, induce failure on detail write after header/counter stage, retry query | Request fails; all three before/after values unchanged; no partial order |
| QA-S4-039 | 52 / 14–18 | SQL/HTTP | A; concurrent-safe Test fixture | Send 12 create requests concurrently, each with unique marker in Test fixture; query codes | Twelve successful orders have 12 distinct sequence/code values and matching counter; no duplicate or partial rows |
| QA-S4-040 | 52 / 14–18 | SQL/HTTP | Test-only counter fixture near 9999 | Create next orders through API | Code preserves sequence 10000 and higher without truncation; unique code |
| QA-S4-041 | 52 / 14–18 | HTTP/SQL | Controlled UTC clock or test host | Create at 16:59:59Z and 17:00:00Z | BusinessDate uses Vietnam date; second belongs to next calendar date; CreatedAt remains UTC |
| QA-S4-042 | 52 / 14–18 | HTTP | A and B with own orders | GET mine with keyword/date; GET B's order ID as A | Mine filters own Code/date and sorts CreatedAt desc then Id desc; foreign detail 404 same as nonexistent |
| QA-S4-043 | 52 / 14–18 | Browser/SQL | Integrated Web/API, A and B | Login A → menu → cart → create → detail → mine → logout; query same order code in DB | One order with matching code/total/status/detail across UI, API and SQL; B cannot read it |
| QA-S4-044 | 52 / 14–18 | Browser/SQL | Integrated Web/API | Double-click create; simulate POST timeout/401 after possible DB commit; inspect cart/mine/SQL | Web does not auto-retry POST or falsely report success; cart retained on uncertain result; user can inspect mine before retry |

## Independent expected-value worksheet

| Input | Calculation | Expected |
|---|---|---:|
| 2 cups, size 30,000; T1×3 at 5,000 and T2×1 at 4,000 **per cup** | `2 × (30,000 + 15,000 + 4,000)` | 98,000 |
| Same configuration, 1 cup after edit | `1 × (30,000 + 15,000 + 4,000)` | 49,000 |
| Add second line of 1 cup at reviewed seed price P, no topping | `98,000 + P` | Determine P from seed before call |

Do not copy the API's returned amount into this worksheet. In particular, the two topping quantities apply to each cup. `totalQuantity` for the two-line example is 3, not 2 lines.

## Execution mode

| Cases | Mode |
|---|---|
| 001–016, 019–022, 027–037, 040–042 | Manual direct HTTP with SQL before/after where the case says HTTP/SQL; automate only after the real endpoint exists |
| 017–018, 023–026, 043–044 | Manual browser; 023–024 may additionally have xUnit unit coverage, which is recorded separately from browser evidence |
| 038–039 | SQL-backed integration with controlled fixture and direct HTTP; concurrency/fault behavior must be observed on SQL Server, not inferred from Moq |

## Execution record format

Append one row per attempted case to `TestResults.md`: ID, timestamp (Asia/Ho_Chi_Minh), runner, FE/API commit, schema/seed revision, environment, expected, actual, status, evidence link, bug link, blocker owner/input. A case may have multiple runs; the latest relevant release-candidate run is used in Test Summary. No token/password/secret in logs or screenshots.
