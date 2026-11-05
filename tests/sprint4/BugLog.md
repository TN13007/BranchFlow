# Sprint 4 QA Bug Log

Updated: 05/11/2026. The integrated review confirmed two defects below. Missing order endpoints/fixtures remain dependency gates, not invented defects. BF-LOCAL IDs are repository-local identifiers, not Jira tickets; Jira was not updated by this review. Earlier setup errors remain recorded in TestResults.md.

| Bug ID / Jira URL | Case ID | Story | Severity | Priority | Owner | Build / environment | Reproduction steps | Expected | Actual | Evidence | Fix SHA | Retest ID / result | State |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BF-LOCAL-001 / no Jira ticket | QA-S4-022 | Menu / SCRUM-51 | Minor | P2 | Khang integration review | `2128070`, local BranchFlow_Test | Valid assigned employee; deactivate assigned branch; GET menu with valid JWT | 404 ProblemDetails; outside scope remains 403 | 403; MenuSmoke stopped after 28 passing checks | [Review/retest record](TestResults.md#run-2026-11-05-h-integrated-review-and-menu-regression-retest) | Fix committed with this record | MenuSmoke 34/34, including inactive/deleted 404 | Retested Pass |
| BF-LOCAL-002 / no Jira ticket | QA-S4-009 | Auth / SCRUM-50 | Major | P1 | Khang integration review | Merged candidate plus initial menu fix, BranchFlow_Test | Login; send simultaneous refresh requests using same token | One 200, one 401; one descendant; reuse revokes session | Intermittent 200/500; SQL 547 during follow-up diagnostic | [Failure output](AuthSmokeResult-2026-11-05-BeforeRefreshFix.txt) | Refresh-clock fix committed with this record | AuthSmoke 13/13; 3 concurrent attempts passed; 59 unit tests | Retested Pass |

Severity describes impact (blocker, major, minor). Priority is agreed with PM. Do not close a bug from a developer's statement alone; record an actual rerun on the fix commit and linked regression checks.
