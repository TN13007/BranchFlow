# SCRUM-131 — three-participant UAT handoff

Prepared by Nguyễn Thuận assisted by Codex, 17/11/2026. This is an **unfilled participant record**, not a stakeholder sign-off. Technical four-role browser smoke is [recorded separately](QA-Public-Retest-2026-11-17.md); the QA-operated demo order does not count as a participant-created order.

PCG participant instructions and case-by-case expected results: [PCG UAT guide](../../docs/PCG-UAT-Guide-2026-11-17.md).

## Session setup

- Coordinator: confirm the deployed Web URL and candidate SHA with Khang before the session. The last user-supplied Web URL was `https://branchflow-team-web-khang153.azurewebsites.net/login`; the last owner-reported deployed candidate was `2844101`, which has not been independently attested as the running binary.
- Invite **three distinct internal people**. Each person operates their own browser session and creates one order. A shared demo login alone does not identify the participant; each person should confirm their result from their own Jira account or another attributable record.
- Do not put passwords, bearer tokens or cookies in the record. Note actual session time, account role, branch, order code, expected/actual and feedback. QA can observe and help, but cannot sign for the participant.

| Participant | Actual session time (VN) | Role / branch | Order code created by participant | Expected / actual, result | Participant's own confirmation link |
|---|---|---|---|---|---|
| 1 — pending | Not Run | Pending | Pending | Pending | Pending |
| 2 — pending | Not Run | Pending | Pending | Pending | Pending |
| 3 — pending | Not Run | Pending | Pending | Pending | Pending |

## Script for each participant

1. Open the confirmed release URL, sign in with the assigned account and verify the displayed role/branch.
2. Create one low-value demo order: record selected product, size, quantity and expected total before confirmation. Confirm once and record the server's order code, total and status.
3. Open **Đơn của tôi** and verify that the same code appears once with the expected total. If a timeout or 401 occurs, check own orders before attempting any retry.
4. Have the participant state Pass/Fail and one sentence of feedback in their own attributable confirmation. If Fail, QA captures reproducible steps, SHA, environment, expected/actual and severity, then links a Jira defect.

For four-role MVP acceptance, coordinator/QA additionally records Owner and Branch Admin scope, System Admin catalog, employee order flow, stock and revenue views with the confirmed release SHA. [Local technical checks](QA-SCRUM-130-2026-11-17.md) and [fault retest](QA-SCRUM-132-Fault-2026-11-17.md) support this review but do not replace participant confirmation. Open local observation [SCRUM-140](https://khangba153.atlassian.net/browse/SCRUM-140) requires FE triage.

Acceptance remains **Not Run for three participants** until all three rows contain genuine outcomes and Khang reviews the UAT record. Do not convert this template into a Pass count or backdate it.
