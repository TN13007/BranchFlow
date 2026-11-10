# Sprint 5 — Management API verification (SCRUM-110/111/112)

## Source and status

- Repository: `TN13007/BranchFlow`
- Intended test base: latest integrated `main` commit `bf69ad0a59943ca554ef1b9740b3da954f2a8698` (verify the current main head before running).
- Related implementation: `ManagementController.cs`, `MvpService.cs`, `MvpRepository.cs`, `BranchAccessService.cs`, and `AuthRepository.cs`.
- Runner: [ManagementApiSmoke.ps1](ManagementApiSmoke.ps1).
- Environment required: local HTTPS API on port 7107, local SQL Server database `BranchFlow_Test`, seeded demo accounts.
- Execution state for this handoff: **Not Run in this workspace**. The workspace does not contain the .NET SDK or SQL Server runtime. This file records test intent and expected outcomes, not a claim of passing HTTP tests.

## What is already present in the core

The integrated source already contains:

1. Branch list/create/update API endpoints in `ManagementController`.
2. SystemAdmin-only branch create/update authorization.
3. Branch list scoping through `MvpService.BranchesAsync` and `BranchAccessService`.
4. User creation restricted to `BRANCH_ADMIN` and `EMPLOYEE` roles by `MvpService.CreateUserAsync`.
5. BranchAdmin profile updates limited by service logic to Employee accounts in an accessible branch; the non-SystemAdmin path does not apply submitted role, active-state, or stock-permission fields.
6. Assignment checks for one effective BranchAdmin per branch and one effective Employee assignment.
7. SQL filtered unique indexes protecting open assignments, plus API mapping of expected assignment conflicts to HTTP 409.
8. Management response DTOs that do not define password or password-hash fields.

These are source-level findings only. They are not a substitute for real HTTP/SQL execution.

## Test matrix

| ID | Actor | Request / operation | Expected result | Status |
|---|---|---|---:|---|
| M-001 | No token | GET `/api/management/branches` | 401 | Not Run |
| M-002 | SystemAdmin | GET branch list | 200; all non-deleted branches | Not Run |
| M-003 | Owner | GET branch list | 200; readable branch data | Not Run |
| M-004 | Owner | POST/PUT branch | 403; no SQL change | Not Run |
| M-005 | Owner | GET management users | 403; endpoint role policy excludes Owner | Not Run |
| M-006 | SystemAdmin | POST branch, then PUT branch | 200; response matches SQL | Not Run |
| M-007 | BranchAdmin 01 | GET branch list | 200; only BF-DEMO-01 | Not Run |
| M-008 | BranchAdmin 01 | GET stock in BF-DEMO-02 | 403 | Not Run |
| M-009 | BranchAdmin 01 | Change availability in BF-DEMO-02 | 403; no SQL change | Not Run |
| M-010 | BranchAdmin 01 | POST branch/user/assignment | 403 | Not Run |
| M-011 | Employee A | GET stock in BF-DEMO-01 | 200 | Not Run |
| M-012 | Employee A | GET stock in BF-DEMO-02 | 403 | Not Run |
| M-013 | Employee A | POST user/assignment | 403 | Not Run |
| M-014 | SystemAdmin | Assign one BranchAdmin to two temporary branches | Both assignments accepted | Not Run |
| M-015 | SystemAdmin | Assign a second effective BranchAdmin to an occupied branch | 409; no second assignment | Not Run |
| M-016 | SystemAdmin | Create Employee with initial branch assignment | 200; account/profile/assignment created | Not Run |
| M-017 | SystemAdmin | Give that Employee a second effective assignment | 409; original assignment retained | Not Run |
| M-018 | BranchAdmin | Update Employee profile in its own branch | 200; allowed profile fields change | Not Run |
| M-019 | BranchAdmin | Submit role/status/branch/stock-permission escalation fields in profile update | Role, active state, assignment and permissions remain unchanged | Not Run |
| M-020 | SystemAdmin / BranchAdmin | Read management user response | No password or password-hash fields | Not Run |

The HTTP status expectations follow the current controller and service implementation; the runner confirms them against the live local API. A 403 can arise either from the controller's role policy or from service-level branch-scope validation. Keep those causes distinct when recording results.

## Run the smoke suite

Use a disposable local test database. Do not run against production, a shared demo database, or a personal database containing work that must be preserved.

1. Configure local API secrets according to [Sprint 4 Auth local setup](../../docs/Sprint4-Auth-Local-Setup.md).
2. Ensure the API connection string targets `BranchFlow_Test`, then run the API with the HTTPS launch profile.
3. Set the demo password in the current PowerShell process only. Do not place it in the script, command history, screenshots, or Jira.

```powershell
$env:BRANCHFLOW_DEMO_PASSWORD = Read-Host 'Demo account password'
./tests/sprint5/ManagementApiSmoke.ps1
Remove-Item Env:BRANCHFLOW_DEMO_PASSWORD
```

The script also checks the SQL connection target. It creates two uniquely named temporary branches, a temporary BranchAdmin, a second BranchAdmin, and a temporary Employee. It checks API and SQL branch values, assignment conflicts, role denials, branch scope, and profile escalation. Its `finally` block logs out test sessions and deletes only the temporary QA-prefixed records created by that run.

If cleanup cannot be confirmed, stop and inspect only `QA110A-*`, `QA110B-*`, and `qa110.*` records in `BranchFlow_Test` before rerunning. Do not reset the database.

## Handoff record to complete after execution

Record the following in the Jira issue comment after a real run:

- Exact source SHA and branch.
- API base URL and confirmed SQL database name.
- PowerShell version and .NET SDK version.
- Runner output with each Expected/Actual result.
- Build and test command exit codes, plus exact test counts.
- Cleanup confirmation.
- Any failed case, linked defect, fix SHA, and retest result.
- Khang's actual review result.

Do not change an issue to Done until the required HTTP/SQL cases have run, failures have been retested, and Khang has reviewed the evidence.
