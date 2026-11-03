# Sprint 4 QA Test Summary — SCRUM-92

Owner: Nguyễn Đặng Hòa Thuận. Updated: 03/11/2026 (Asia/Ho_Chi_Minh). Candidate source commit: `ca9b56e593278595bcf1de250e222c6f39056439` on `origin/khang`. PM acceptance review: pending Khang.

| Catalog cases | Pass | Fail | Blocked | Not Run | Skipped |
|---:|---:|---:|---:|---:|---:|
| 44 | 0 | 0 | 0 | 44 | 0 |

Counts reflect the explicit per-case initial state in [TestCases.md](TestCases.md). The [preflight](TestResults.md) found runtime gates and did not execute individual catalog cases, so it does not turn them into Pass or Fail. The existing repository xUnit suite ran **23/23 Pass** on the inspected commit, including a local TRX run, and solution build passed after restore. Those 23 tests are a separate baseline observation and are not added to the 44 Sprint QA case count.

## Current assessment

**Not ready for Sprint 4 demo acceptance on the inspected commit.** Auth and order API, demo seed and local SQL test environment are missing from this candidate; browser E2E and transaction proof cannot yet be run. The menu service/Web unit and transport tests provide limited confidence for those components only. No runtime defect is confirmed, and no bug has been closed or retested.

| Workstream | Evidence available | Remaining gate |
|---|---|---|
| SCRUM-87 Test Plan | [TestPlan.md](TestPlan.md) | Khang review and shared evidence link |
| SCRUM-88 Cases / expected | [TestCases.md](TestCases.md), independent 98,000 worksheet | Khang review; resolve actual seed IDs before execution |
| SCRUM-89 Auth/permissions | [TestResults.md](TestResults.md) preflight only | BE1 auth API/seed; FE browser integration; direct HTTP results |
| SCRUM-90 Price/validation/persistence | Existing 23-test baseline and [TestResults.md](TestResults.md) | BE2 order service/API, SQL Test DB, HTTP 400 and transaction results |
| SCRUM-91 E2E | Case definitions QA-S4-043/044 | Integrated FE/API/DB, same-code UI/API/SQL evidence |
| SCRUM-92 Retest/summary | This summary and [BugLog.md](BugLog.md) | Fix commits and original-case retests; release-candidate smoke |

## Retest log

| Bug ID | Original case | Fix SHA | Related regression cases | Time / runner | Expected | Observed actual | Evidence | Result |
|---|---|---|---|---|---|---|---|---|
| — | — | — | — | — | — | No fix/retest received | — | Not Run |

## Evidence link checklist before handoff

- [ ] PR/commit link for this QA package opens for Khang; local paths alone are not shared evidence.
- [ ] Jira SCRUM-87/88 link the reviewed plan/cases; reviewer comment is recorded.
- [ ] SCRUM-89/90/91 results cite exact FE/API SHAs, DB schema/seed, date, expected/actual and redacted evidence.
- [ ] Every bug links its failing case, owner, fix SHA and actual retest on that SHA.
- [ ] Final counts are recalculated from latest relevant case runs; demo readiness is reviewed by Khang.

No Jira status, deadline, worklog, bug or attachment was changed by this local QA preparation. Do not mark the stories or runtime QA tasks Done from the baseline tests.
