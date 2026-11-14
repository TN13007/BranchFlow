# Sprint 4 order catalog mapping — 12/11/2026

Independent local runner `tests/OrderSmoke` against application tree `0e0c514` on `Thuanaj`, Windows/.NET 10 Release, SQL Server `localhost\SQLEXPRESS` / `BranchFlow_Test`. The runner starts its own API, creates random fixture IDs, sends real HTTP requests, checks SQL and removes its fixture. [Raw runner output](QA-Order-Catalog-Expanded-2026-11-12.txt) and [catalog expected values](../sprint4/TestCases.md) are the evidence. Assertions below are mapped to each entire catalog case, not counted by the number of HTTP calls.

| Case | Observed actual on this source | Result |
|---|---|---|
| QA-S4-015 | Owner menu/create 403; assigned Employee menu 200; no order/counter write | Pass |
| QA-S4-016 | Employee assigned to fixture branch A tried Demo 02 branch B: 403, B order/counter unchanged | Pass |
| QA-S4-027 | POST 201/Location detail; 98,000đ = 2 × (30,000 + 3×5,000 + 4,000); COMPLETED and SQL header/item/toppings/note persisted | Pass |
| QA-S4-028 | POST two lines 201; detail retained two distinct topping/note configurations, three cups, 131,000đ after price update | Pass |
| QA-S4-029 | Empty/null items, null line/toppings/notes returned 400; SQL order/counter remained absent | Pass |
| QA-S4-030 | Quantity 0/negative/fraction/string and nonpositive ID returned 400; SQL order/counter remained absent | Pass |
| QA-S4-031 | Unknown positive ProductSizeId and ProductId/SizeId instead of ProductSizeId returned 400; no order | Pass |
| QA-S4-032 | Topping quantities 1, 2, 3 created orders with server totals; 0/4/duplicate topping returned 400 and did not allocate counter | Pass |
| QA-S4-033 | Duplicate note and two notes in the same group returned 400 with no order; empty notes created an order and detail preserved empty notes | Pass |
| QA-S4-034 | Root `price`, `totalAmount`, `createdByUserId`, `status` each returned 400 before any order/counter write | Pass |
| QA-S4-035 | Nested item `unitPrice`/`subtotalAmount` and topping `price` each returned 400 before any order/counter write | Pass |
| QA-S4-036 | After ProductSize and Topping SQL price changes, new POST totals and persisted item/topping unit-price snapshots used current DB values | Pass |
| QA-S4-037 | After master product, note, topping name/price changes, GET of old order retained original names/prices and 98,000đ total | Pass |
| QA-S4-038 | Branch-only SQL trigger rejected detail insert with HTTP 500; SQL header count and daily counter stayed unchanged; no partial item | Pass |
| QA-S4-039 | Twelve simultaneous POSTs returned twelve distinct codes; SQL order count and daily counter each increased by exactly twelve | Pass |
| QA-S4-040 | Counter pre-set to 9999 in fixture; next API code ended `-10000`, with exactly one matching SQL order | Pass |
| QA-S4-041 | Controlled 16:59:59Z/17:00:00Z HTTP inserts produced adjacent Vietnam BusinessDates; SQL CreatedAt remained exact UTC | Pass |
| QA-S4-042 | Mine keyword/date matched one SQL order; full own list sorted CreatedAt/Id descending; foreign and nonexistent detail each returned 404 | Pass |

Order smoke checks: see the final `RESULT` line in the raw output. These 18 cases combine with 14 AuthSmoke and 4 MenuSmoke cases on the same application tree, giving **36 of 44 formal cases Pass**. The eight browser cases QA-S4-017, 018, 023–026, 043 and 044 remain to run and record on a single final candidate. This mapping does not use older UI evidence to inflate the same-source count. It is local Test DB evidence; public smoke and participant UAT are separate gates.
