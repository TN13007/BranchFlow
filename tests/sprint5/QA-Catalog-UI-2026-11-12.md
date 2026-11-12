# QA catalog and System Admin UI — 12/11/2026

Runner: Nguyễn Đặng Hòa Thuận assisted by Codex. Existing branch `Thuanaj`, source `520a7ef` (application tree from integrated main `0e0c514`). Windows/.NET 10 Release, isolated runner APIs and real Web/API at HTTPS localhost against `BranchFlow_Test`. Public environment was not used. Authentication, menu and order runners generated/removed their own SQL fixtures. Their raw, credential-free outputs are [Auth](QA-Auth-Catalog-2026-11-12.txt), [Menu](QA-Menu-Catalog-2026-11-12.txt) and [Order](QA-Order-Catalog-2026-11-12.txt).

| Formal Sprint 4 catalog scope | Expected | Observed actual | Result on this source |
|---|---|---|---|
| QA-S4-001–014 | Login, validation/rate limit, refresh/reuse/concurrency, logout, lock, TTL and four-role scope | AuthSmoke explicitly printed PASS for all 14 IDs; 14 catalog cases, 0 failed; fixture cleaned | Pass, 14 IDs |
| QA-S4-019–022 | Valid menu structure, active/branch filters, bad query and foreign/inactive branch denial | MenuSmoke explicitly printed PASS for these four IDs; 34 HTTP assertions, fixture cleaned | Pass, 4 IDs |
| QA-S4-041 | Orders at 16:59:59Z and 17:00:00Z have distinct Vietnam BusinessDates and UTC CreatedAt | OrderSmoke directly labelled three QA-S4-041 assertions for SQL date, UTC and HTTP; all Pass | Pass, 1 ID |
| Other order API/SQL cases 027–040 and 042 | Price, validation, snapshot, rollback, concurrency, counter and own history | OrderSmoke 106/106 HTTP/SQL assertions Pass; fixture cleaned. The checks overlap these cases but do not prove every listed subcondition one-to-one. | Partial catalog mapping; **not** counted as 15 full case Pass |

This first catalog pass established 19/44 explicitly labelled cases. A later [per-condition order mapping](QA-Catalog-Order-Mapping-2026-11-12.md) expanded the same-application-source result to **36/44 Pass, 8 browser cases pending** after strengthening and rerunning OrderSmoke. Neither count is inferred solely from a runner's total assertion count. The earlier formal ledger's 5 Pass/39 Not Run was a prior candidate snapshot and remains part of history.

The subsequent [same-source browser/SQL retest](QA-Browser-2026-11-12.md) raised the catalog result to **43/44 Pass**. QA-S4-018 remains Partial pending a direct old Web-session API access-token 401 check.

## Real System Admin browser check, local Test DB

A dedicated temporary System Admin account was created only in `BranchFlow_Test` because the nine demo accounts store Identity V3 password hashes, not recoverable plaintext. A first local hash-generation command selected the wrong .NET overload and made an unusable temporary hash; it was replaced with a valid Identity V3 hash before login. API login was checked, then the actual Edge Web UI signed in and opened **Danh mục**. No demo password was read or changed.

| UI path | Expected | Observed actual | Result |
|---|---|---|---|
| Save blank product | Invalid form rejected | “Chọn danh mục hợp lệ”; no product created | Pass |
| Create product and M size | Unique name, category, M/29,000đ saved | UI showed “Đã lưu danh mục” and `QA UI 12-11 Coffee`, M 29,000đ | Pass |
| Negative and valid price edit | -1 rejected, 30,000đ accepted | Error “Giá trị số vượt giới hạn…” for -1; later success showed M 30,000đ | Pass |
| Stop selling product | Product remains listed, inactive | UI showed “Tắt toàn hệ thống” for QA product | Pass |
| Duplicate product | Same name/category rejected | UI showed “Mã, tên hoặc phân công đã tồn tại”; only one QA product in SQL | Pass |
| Topping negative and valid price | -1 rejected, unique topping/6,000đ saved | Same numeric validation error; success showed topping and 6,000đ | Pass |
| Duplicate topping | Same name/group rejected | UI showed duplicate error; only one QA topping in SQL | Pass |
| Logout | Session ends | Web returned to `/login?notice=logout` with “Bạn đã đăng xuất.” | Pass |

SQL before cleanup identified exactly one QA account (Id 141), product (Id 35), M price (Id 44) and topping (Id 38). Neither QA catalog item was referenced by any order item, there were no branch-product/topping rows or account audit rows, and the account had two test sessions/refresh rows. A guarded transaction deleted those exact QA rows and their sessions/tokens; a follow-up SQL count returned **0 QA fixture rows**. The locally started API and Web processes were stopped. This is a focused System Admin UI/SQL pass for WBS 3.3.1.6, not a full four-role UAT or public test. No screenshot artifact was captured; the observed browser states and SQL checks are recorded here.

Remaining gates: complete formal 44-case catalog on one final release candidate, four-role UI coverage, three distinct internal participants who each create an order, public smoke using Khang's deployed HTTPS API/Web URLs plus matching SHA/manifest, and PM/stakeholder acceptance. The 13/11 deadline remains at risk until those inputs and review are available.
