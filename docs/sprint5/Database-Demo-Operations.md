# Sprint 5 — Demo database and operations handoff (SCRUM-114)

## Current baseline and existing evidence

The following evidence is already present in integrated `main` at `bf69ad0a59943ca554ef1b9740b3da954f2a8698`. It was contributed under the separate WBS / Jira mapping documented in [WBS-5.1.1-Handoff.md](../../docs/WBS-5.1.1-Handoff.md); it is reused here as an existing input, not claimed as new work by this task.

| Evidence | Existing recorded result |
|---|---|
| DemoSeed on `BranchFlow_Dev` | Initial run and rerun logged; fixture has four orders, six stock transactions, three branch balances; seeded account fingerprints unchanged |
| DemoSeed on `BranchFlow_Test` | Initial run and rerun logged; same fixture assertions; existing accounts, passwords, and account state unchanged |
| API / SQL verifier | Four role logins; three demo branches; branch scope; order snapshots; daily revenue compared with SQL; stock values checked |
| UI evidence | Four selected Test environment screenshots are committed under `docs/evidence/sprint5-uat/` |
| Public environment | Not part of these records; public URL and public smoke were not established by the local Test/Dev evidence |

Exact files:

- [DemoSeed Test initial](../../tests/sprint5/DemoSeed-Test-2026-11-10.txt)
- [DemoSeed Test rerun](../../tests/sprint5/DemoSeed-Test-Rerun-2026-11-10.txt)
- [DemoSeed Dev initial](../../tests/sprint5/DemoSeed-Dev-2026-11-10.txt)
- [DemoSeed Dev rerun](../../tests/sprint5/DemoSeed-Dev-Rerun-2026-11-10.txt)
- [API / SQL result](../../tests/sprint5/DemoApiResult-2026-11-10.txt)
- [Seed wrapper](../../tools/Seed-WbsDemo.ps1)
- [SQL fixture](../../database/005-wbs-demo-data.sql)
- [API / SQL verifier](../../tests/sprint5/Verify-WbsDemoData.ps1)

## Database separation

| Database | Intended use | Allowed operation |
|---|---|---|
| `BranchFlow_Dev` | Member's local development data | Local development and approved demo seed |
| `BranchFlow_Test` | SQL integration/API/QA fixtures | Test and verification only |
| Production / hosted DB | Deployed demo or real environment | Only approved deployment and backup procedure; never use local seed/reset commands |

Do not reset an existing local database to make a test pass. The schema baseline checks that the target database is empty and refuses to run over an existing schema.

## Preparation sequence for a fresh disposable database

Only follow this sequence when the named database was intentionally created for this purpose and does not already contain valuable data.

1. Create an empty local database using the required collation, as described in [database/README.md](../../database/README.md).
2. Run `database/001-baseline.sql` exactly once on that empty database.
3. Run `database/002-reference-data.sql`; it inserts only missing roles, sizes, and ingredient units.
4. Run the existing `tools/BranchFlow.DemoSeed` program in `Development` or `Testing`. It creates only missing demo accounts/branches/assignments and does not overwrite existing passwords on an ordinary seed run.
5. Run `tools/Seed-WbsDemo.ps1 -Database BranchFlow_Test` (or Dev after Test) to add the local WBS demo menu/order/stock fixture. The wrapper refuses non-local servers and other database names, checks the nine demo account fingerprints before/after, and fails rather than overwriting a conflicting fixture.
6. Run `tests/sprint5/Verify-WbsDemoData.ps1 -Database BranchFlow_Test` with the API connected to the same database. It verifies four roles, branches, fixture data, API scope, and SQL values.

Do not run `001-baseline.sql` against an existing database. Do not use `--reset-demo-passwords` unless the group has explicitly decided to rotate all nine demo account passwords and the credentials are distributed through the approved private channel.

## Backup and restore

Back up the disposable Test database before adding a large fixture or changing any demo data. The SQL Server service account must have write access to the backup directory. Use a new, unique backup filename for every backup; do not use a shared filename that could overwrite another person's backup.

```sql
BACKUP DATABASE [BranchFlow_Test]
TO DISK = N'<unique-local-backup-path>\BranchFlow_Test_<date-time>.bak'
WITH COPY_ONLY, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY
FROM DISK = N'<same-unique-local-backup-path>\BranchFlow_Test_<date-time>.bak'
WITH CHECKSUM;
```

To test recovery without overwriting the original database:

1. Read the logical data/log names from the backup using `RESTORE FILELISTONLY`.
2. Check that a separate scratch database name and separate MDF/LDF paths do not already exist.
3. Restore into that new scratch database using `WITH MOVE` for both logical files.
4. Run `DBCC CHECKDB` on the restored scratch database, then run the required smoke checks against that copy.
5. Remove only the scratch restore database after the owner confirms the recovery check is complete. Never drop `BranchFlow_Test`, `BranchFlow_Dev`, or a hosted database as part of this procedure.

Example restore shape (replace the logical names and paths with the values returned by `RESTORE FILELISTONLY`):

```sql
RESTORE FILELISTONLY
FROM DISK = N'<unique-local-backup-path>\BranchFlow_Test_<date-time>.bak';

RESTORE DATABASE [BranchFlow_Test_Restore]
FROM DISK = N'<unique-local-backup-path>\BranchFlow_Test_<date-time>.bak'
WITH
    MOVE N'<logical-data-name>' TO N'<new-scratch-data-path>\BranchFlow_Test_Restore.mdf',
    MOVE N'<logical-log-name>'  TO N'<new-scratch-log-path>\BranchFlow_Test_Restore_log.ldf',
    RECOVERY, CHECKSUM, STATS = 10;

DBCC CHECKDB (N'BranchFlow_Test_Restore') WITH NO_INFOMSGS;
```

## Configuration and secret handling

- Keep database connection strings in User Secrets for local API development or in the hosting platform's secret store for deployment.
- Keep the demo password in a process environment variable only while the verifier is running: `BRANCHFLOW_DEMO_PASSWORD`.
- Never put the demo password, database password, JWT key, access token, refresh token, or password hash in Git, Jira, terminal screenshots, logs, or a release archive.
- Do not run `dotnet user-secrets list` while capturing handoff evidence.
- Do not send a production connection string to a test runner.

## Fixture retention and cleanup boundary

The `BF-WBS5-v1` fixture is intentionally repeatable and verifies that a rerun does not create duplicate records. Its SQL script does **not** automatically delete the four demo orders, six stock transactions, or the demo ingredient after verification.

This is deliberate: those records may be used by QA/UAT and a demo. Do not delete them or reset the database without Khang's explicit decision and a verified backup. The management API smoke runner in the separate SCRUM-110/112 test branch creates its own temporary `QA110*` records and cleans only those records in its `finally` block; it does not clean the shared WBS demo fixture.

## Handoff checklist

| Item | Evidence / state |
|---|---|
| Schema / reference seed instructions | Present in `database/README.md` |
| Existing Dev and Test seed run | Logs committed; see links above |
| Idempotent WBS demo fixture | Present; initial and rerun logs show no duplicates |
| API / SQL cross-check | Existing result log committed |
| Backup / restore procedure | Documented in this handoff; restore drill still needs execution by the database owner |
| Secrets excluded from repo | Configuration guidance documented; confirm on the actual deployment |
| Production / public DB verification | Not Run; not implied by local Dev/Test evidence |
| Cleanup of shared WBS fixture | Deferred until Khang confirms it is no longer needed |

SCRUM-114 should remain open until Khang receives this guide, confirms the intended DB retention point, and any required restore drill is actually executed and recorded.
