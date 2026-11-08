# Sprint 4 QA Test Summary — SCRUM-92

Owner: Nguyễn Đặng Hòa Thuận. Updated: 09/11/2026 (Asia/Ho_Chi_Minh). Current tested core: `Thuanaj` at `e83ccb8` (merged `main`). [The 09/11 retest](TestResults.md#run-2026-11-09-m-merged-core-retest) independently passed former failing QA-S4-044, including 401 and timeout after the order commit. The last **complete catalog snapshot** remains the 06/11 run below (43 Pass / 1 Fail on an older core). The other 43 cases were not all rerun on the same new SHA, so one-build 44/44 acceptance remains pending. [WBS tracker](WBS-QA-2026-11-09.md) and [local UAT record](UAT-2026-11-09.md) separate verified work from remaining scenarios and public smoke.

On the current core, Release build passed with 0 warnings/errors; SQL-enabled full suite **81/81 passed**, twice. AuthSmoke passed 14/14 catalog cases; MenuSmoke 34/34 HTTP checks; OrderSmoke 106/106 checks; RevenueSmoke 11/11 checks. These checks cannot be summed as catalog cases. Local four-role UI spot checks passed for inspected screens. Public smoke and stakeholder acceptance have not run.

## Historical complete-catalog snapshot — 06/11/2026

| Catalog cases | Pass | Fail | Blocked | Not Run | Skipped |
|---:|---:|---:|---:|---:|---:|
| 44 | 43 | 1 | 0 | 0 | 0 |

The fresh Release unit run passed **65/65** after the machine owner disabled Windows Application Control. The [Auth TTL run](AuthSmokeResult-2026-11-05-Ttl.txt) passed **14/14 catalog cases**, and the [Order boundary run](OrderSmokeResult-2026-11-05-Boundary.txt) passed **106/106 checks**. Browser/SQL execution on 06/11 passed QA-S4-017 and 026. QA-S4-044 failed its 401-after-commit branch: the Web does not warn that the create result is uncertain, and the cart is lost on relogin. See [BF-LOCAL-003](BugLog.md).

The earlier Windows Application Control interruption is historical; the machine owner changed that setting and the 06/11 unit run and demo seed completed. The two fault-injection orders remain only in `BranchFlow_Test` as local evidence.

## Historical review snapshot before order and browser integration

The section below describes the earlier `2128070` review, when 18 cases passed. Its count and API-readiness statements are retained only as history.

Owner: Nguyễn Đặng Hòa Thuận. Updated by Codex under Khang's authorized review/fix on 05/11/2026 (Asia/Ho_Chi_Minh). Candidate at this snapshot: merged base `2128070` plus the branch-scope correction. Sprint acceptance was pending.

| Catalog cases | Pass | Fail | Blocked | Not Run | Skipped |
|---:|---:|---:|---:|---:|---:|
| 44 | 18 | 0 | 0 | 26 | 0 |

Counts use the 44-row catalog and run H in [TestResults.md](TestResults.md): QA-S4-001–012 and 014 passed integrated auth SQL/HTTP execution; QA-S4-019–022 passed menu SQL/HTTP retest; QA-S4-023 passed the cart unit fixture. The menu runner's 34 HTTP checks map to four catalog cases, not 34 catalog passes. The full xUnit suite passed **59/59**. QA-S4-024's unit portion passed but its UI step has not run, so it remains Not Run. These 18 passes do not establish full Sprint acceptance.

## Assessment at historical snapshot

**Not ready for full Sprint 4 acceptance.** Auth endpoints, shared branch scope and the account demo seeder are now merged. Menu status regression and intermittent simultaneous-refresh 500 were reproduced, corrected and successfully retested. Integrated auth SQL/HTTP passed 13 catalog cases and menu passed 34 checks. The seeder has unit coverage but was not executed as part of this review; auth/menu smoke uses isolated fixtures instead. Create-order API, priced demo-menu readiness, browser E2E and order transaction proof remain unverified or unavailable. See [BugLog.md](BugLog.md) for both defects and retests.

| Workstream | Evidence available | Remaining gate |
|---|---|---|
| SCRUM-87 Test Plan | [TestPlan.md](TestPlan.md), reviewed 04/11; Jira Done | Documentation scope complete; runtime tests are separate tasks |
| SCRUM-88 Cases / expected | [TestCases.md](TestCases.md), independent 98,000 worksheet; UI quantity-zero expected corrected; Jira Done | Documentation scope complete; resolve actual seed IDs before runtime execution |
| SCRUM-89 Auth/permissions | [Integrated auth SQL/HTTP run](AuthSmokeResult-2026-11-05-IntegratedReview.txt): 13 case passes after merge | Natural TTL expiry fixture, create-order permission checks and browser integration |
| SCRUM-90 Price/validation/persistence | 59-test suite and [SQL baseline verification](DatabaseSetupResult-2026-11-03.txt) | BE2 order service/API and priced menu seed; HTTP 400 and order transaction results |
| SCRUM-91 E2E | Case definitions QA-S4-043/044 | Integrated FE/API/DB, same-code UI/API/SQL evidence |
| SCRUM-92 Retest/summary | This summary, [BugLog.md](BugLog.md) and integrated auth/menu reruns | Remaining catalog execution and browser/order release-candidate checks |

## Retest log

| Bug ID | Original case | Fix SHA | Related regression cases | Time / runner | Expected | Observed actual | Evidence | Result |
|---|---|---|---|---|---|---|---|---|
| BF-LOCAL-001 | QA-S4-022 | Branch-scope fix in same commit as this record | QA-S4-014, 019–022 | 05/11/2026 / Codex for Khang | Assigned inactive/deleted branch 404; outside scope 403 | Both 404 checks passed; outside-scope guards retained | [34-check menu retest](MenuSmokeResult-2026-11-05-ReviewFix.txt) | Pass |
| BF-LOCAL-002 | QA-S4-009 | Refresh-clock fix in same commit as this record | QA-S4-007–012 | 05/11/2026 / Codex for Khang | Concurrent refresh 200/401; one descendant; reuse revokes session | Three attempts passed; full auth runner 13/13 | [Integrated auth retest](AuthSmokeResult-2026-11-05-IntegratedReview.txt) | Pass |

## Evidence link checklist before handoff

- [x] [PR #12](https://github.com/TN13007/BranchFlow/pull/12) and published QA commits are linked for Khang's review.
- [x] Jira SCRUM-87/88 link the reviewed plan/cases and are Done after the 04/11 review.
- [x] Integrated auth/menu review records source base plus same-commit fix, local DB/fixture, expected/actual and redacted output; no Jira update in this review.
- [ ] SCRUM-90/91 order/browser results cite their executed FE/API versions and evidence.
- [ ] Every bug links its failing case, owner, fix SHA and actual retest on that SHA.
- [ ] Final counts are recalculated from latest relevant case runs; demo readiness is reviewed by Khang.

The original local preparation did not update Jira. Review follow-up on 04/11 records the corrected documentation and runtime gates on SCRUM-87–92. Stories/runtime tasks are not accepted from baseline tests.

## Review verification 04/11/2026

The following sections are historical executions. Use the current assessment above and the latest integrated run in TestResults.md for the present candidate.

Build Release: 0 warnings, 0 errors; 23 xUnit tests passed; [34 SQL/HTTP checks passed](MenuSmokeResult-2026-11-04.txt), with per-run fixture cleanup. Four catalog cases QA-S4-019–022 remain Pass, 40 remain Not Run. The revised smoke verifies a separate active note group remains visible while only the tested group's ID disappears. This validates fixture isolation, not completion of auth/order/E2E gates. Historical unavailable SHA values are retained with a traceability correction and links to published source; the new run pins an accessible test/API commit.

## Integration check 05/11/2026

`origin/main` at `d66121f` was fast-forwarded into `Thuanaj` and pushed. The [new run](MainSyncResult-2026-11-05.txt) passed 24/24 xUnit tests, built the API with 0 warnings/errors, and passed all 34 menu SQL/HTTP checks after building the API DLL in this worktree. The four menu catalog cases remain Pass; 40 cases remain Not Run. The current integrated API has no auth or create-order controller. A separate auth feature branch at `6e603d9` passes 55/55 xUnit tests and builds, but is not part of this integrated candidate; direct auth HTTP and order/E2E acceptance still await integration and fixtures.

## Preliminary auth execution 05/11/2026

The [AuthSmoke runner](../AuthSmoke/README.md) at `dd0708d` exercised the API at feature SHA `6e603d9` with live local SQL/HTTP, then cleaned all fixture users and branches. [Run evidence](AuthSmokeResult-2026-11-05.txt) records **13/13 Pass, 0 Fail** for QA-S4-001–012 and 014. This is useful evidence for BE1 review but is excluded from the integrated Pass count. QA-S4-013 still needs natural TTL expiry; 015–016 need create-order actions; 017–018 need browser integration. Rerun the 13 cases on the combined release candidate before changing their integrated statuses.

## Cart unit execution 05/11/2026

On integrated `Thuanaj` test source `b59a1df`, the [named cart tests](CartUnitResult-2026-11-05.txt) and full 26/26 xUnit suite passed. QA-S4-023 is now Pass: 98,000 for two cups, 49,000 after editing the selected line to one cup, with the other line unchanged and removal isolated. QA-S4-024's invalid explicit topping/note and accepted quantities 1–3 passed at unit level, but the UI decrement-to-zero step remains unrun; the case remains Not Run. Current integrated totals are **5 Pass / 39 Not Run**.
