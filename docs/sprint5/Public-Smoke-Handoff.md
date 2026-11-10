# Sprint 5 — Public deployment smoke handoff (SCRUM-115)

## Responsibilities

- Thuần owns preparation of the release evidence bundle and this smoke procedure.
- Khang performs the actual deployment and supplies the API URL, Web URL, and deployed release SHA.
- Thuận / QA performs the public smoke after receiving those deployment details.
- Local Test/Dev results are not public deployment evidence. A successful local smoke does not establish a deployed URL or stakeholder acceptance.

## Inputs required from deployment owner

| Input | Required value |
|---|---|
| API base URL | Actual deployed HTTPS origin, e.g. the approved API hostname |
| Web base URL | Actual deployed HTTPS origin |
| Deployed SHA | Exact source SHA reported by Khang for the deployment |
| Expected SHA | `sourceCommit` from the release manifest |
| Demo credentials | Existing demo password delivered through the group's approved private channel |
| Release package | API and Web ZIP archives plus `release-manifest.json` |

The smoke runner refuses HTTP, localhost/loopback URLs, and a deployed SHA that differs from the release manifest SHA.

## Run public HTTP smoke

Run from the checked-out repository with PowerShell 7+:

```powershell
$env:BRANCHFLOW_DEMO_PASSWORD = Read-Host 'Demo account password'

./tests/sprint5/PublicSmoke.ps1 `
  -ApiBaseUrl 'https://<deployed-api-host>' `
  -WebBaseUrl 'https://<deployed-web-host>' `
  -ExpectedSha '<sourceCommit from release-manifest.json>' `
  -DeployedSha '<exact SHA supplied by Khang>'

Remove-Item Env:BRANCHFLOW_DEMO_PASSWORD
```

The runner checks:

- Web root returns HTTPS HTML.
- API returns 401 when the management branch endpoint is called without a token.
- SystemAdmin and Owner can read demo branch data; Owner cannot create branches.
- BranchAdmin 01 is scoped to Demo 01 and cannot read Demo 02 stock or create assignments.
- Employee A is scoped to Demo 01 and cannot read Demo 02 stock or create accounts.
- Test sessions are logged out in the `finally` block.
- Result JSON is written under `artifacts/public-smoke/`; it contains the provided SHA, public URLs, expected/actual status results, and no tokens or passwords.

This is an HTTP smoke test. It does not run browser E2E, compare public SQL data, certify backup/restore, or obtain stakeholder acceptance. Those checks require their own execution and evidence.

## Current state

- Public API URL: Not supplied / Not verified.
- Public Web URL: Not supplied / Not verified.
- Deployed SHA: Not supplied / Not verified.
- Public smoke: Not Run.
- Actual deploy: assigned to Khang, not Thuần.

Keep SCRUM-115 open until Khang's deployment details are received, the public smoke has actually run on that exact SHA, failures are linked and retested, and the handoff clearly separates preparation, deployment, and QA contributions.
