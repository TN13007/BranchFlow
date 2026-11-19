# QA public retest — 17/11/2026

Runner: Nguyễn Đặng Hòa Thuận assisted by Codex. User supplied the public Web URL and shared demo credential. Passwords, bearer tokens and cookies are not stored in this report or the [credential-free HTTP result](QA-Public-Smoke-2026-11-17.json).

## Source and environment

- Existing branch `Thuanaj` fast-forwarded to `origin/main` `8f56909` before testing; no new branch was created. The [14/11 release manifest](release-2026-11-14/release-manifest.json) and deployment-owner handoff name public candidate `28441013318ffee1a6b8541f1318dab9ca124201`, Web `https://branchflow-team-web-khang153.azurewebsites.net`, API `https://branchflow-team-api-khang153.azurewebsites.net`.
- The deployed SHA is an **owner-supplied assertion**. HTTP/browser smoke cannot attest the running Azure binaries; main `8f56909` is newer than the reported deployed candidate. Do not present local main tests as tests of the deployed binary.
- After merging main, .NET 10 Release solution build succeeded with **0 warnings/0 errors**. Local `BranchFlow_Test` SQL-enabled xUnit suite passed **127/127**, 0 failed, 0 skipped. This verifies the merged local tree.

## Independent public HTTP smoke

`tests/sprint5/PublicSmoke.ps1` checked the four demo roles on HTTPS. The final run completed at `2026-11-17T15:07:34Z` (**22:07:34 VN**) with **27/27 Pass, exit 0**: Web HTML 200, anonymous API 401, all four logins and `/api/auth/me` 200, three demo branches, Owner read-only restrictions, and Branch Admin/Employee branch scope and denied writes. The runner logged out its API sessions and did not persist tokens.

An initial attempt at `15:03:47Z` got Web 200 and anonymous API 401, then timed out on the first API login after 30 seconds. An immediate subsequent warm run passed 27/27; one further rapid repeat hit the configured Owner login rate limit, waited its one-minute window and also passed 27/27. This is an intermittent startup/response-time observation, **not a reproduced product defect**. Recheck cold-start login latency if the Azure app is restarted or idles before the demo.

## Public browser journey

The real Web UI was exercised with the shared demo accounts, with each session logged out afterward:

| Role | Expected / observed actual | Result |
|---|---|---|
| Employee A | Logged into BF-DEMO-01; menu showed 70 open products. Added `Demo - Cà phê sữa` size M, one cup, preview 29,000đ. One confirmation created `/orders/367`, code `BF-DEMO-01-20261117-0001`, `COMPLETED`, 29,000đ; own-orders listed it first. | Pass |
| Owner | Dashboard for 17/11 showed 1 completed order and 29,000đ revenue from Demo 01, matching the new order. Employee directory listed demo staff; filtering Demo 01 left its scoped entries. | Pass |
| Branch Admin 01 | Dashboard exposed only Demo 01 and showed 1 completed order/29,000đ. Orders list showed the new code, Demo Employee A, 29,000đ and completed status. | Pass |
| System Admin | Catalog opened and loaded the shared product list, including the demo coffee at M 29,000đ and an inactive sample; no catalog mutation was performed on public. | Pass |

The one order created during this smoke **remains in the shared demo database** as auditable evidence; the order was not deleted or altered. The agent's demo account activity is **not** counted as any of the three distinct internal people required for stakeholder UAT.

## Remaining acceptance

- `SCRUM-131` remains In Progress: no recorded sign-off from three distinct internal participants each creating their own order. They must use their own sessions and confirm outcomes; shared demo account use by QA cannot establish their identity.
- `SCRUM-132` and `SCRUM-107` are In Review. PM/stakeholder approval remains separate from these technical passes. `SCRUM-133`, `SCRUM-115` and `SCRUM-138` are already Done in Jira based on the 14/11 handoff; this independent retest adds fresh evidence without changing their historical result.
- No direct public SQL inspection or remote binary attestation was performed. The 44/44 Sprint 4 local catalog pass was on the earlier integrated application tree; it is not relabelled as 44/44 on the deployed candidate.
