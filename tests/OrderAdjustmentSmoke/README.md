# SCRUM-124 / WBS3.4.2.1 completion checks

Run from the repository root with .NET10 and a baseline/reference/DemoSeed local BranchFlow_Test. No historical UI order or fixed order ID is required. This is a manual regression harness, outside the standard unit-test suite.

```powershell
dotnet build src/BranchFlow.Api -c Release
dotnet build tests/OrderAdjustmentSmoke -c Release
$env:BRANCHFLOW_TEST_CONNECTION_STRING = 'Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True'
dotnet run --project tests/OrderAdjustmentSmoke -c Release --no-build -- src/BranchFlow.Api/bin/Release/net10.0/BranchFlow.Api.dll
```

Port17207 must be free. The harness starts only its own loopback API process, with Testing environment, normal signature/session/role validation and a random ephemeral signing key. It creates synthetic sessions for existing demo users, so it tests HTTP authorization and business behavior, **not password login**. It does not use the running Dev/Test host's signing key. No key/token/password is printed or committed. Its sessions are revoked and its process is stopped in finally.

It creates its own uniquely named category/product/prices/toppings/note and one SQL order fixture. Code WBS3421-* and a high test sequence are fixture identifiers, not outputs of the sales-order code allocator. It removes only its own menu, order, history and sessions in finally. Existing accounts/passwords, menu and historical evidence orders remain unchanged. Named local SQL Server instances are accepted; remote databases and MARS are rejected.

The first section sends real HTTP requests through the built API: anonymous/Employee/Owner/cross-branch denial, Admin/System success, invalid input, stale quantity and stale cancellation, cancellation/retry and actual revenue delta. SQL snapshots compare state and history after failed writes. Every request uses the ID returned for this run's fixture, rather than a historical order ID.

Rollback cases call the real MvpService, MvpRepository, UnitOfWork and SQL Server directly. A test-only SaveChangesInterceptor injects an impossible foreign key in OrderItem or OrderAdjustment. Actual SQL547 is required; then an independent context checks header/detail/toppings/notes/history/rowversion are unchanged. No trigger, schema change, app-code hook or mocked successful rollback is used. These two cases are service/SQL integration checks, **not HTTP500 or browser fault-injection tests**.

Recorded successful run10/11/2026: 52 checks, fixture WBS3421-e5356f5b170f / ID34, CANCELLED196000 and three history rows. Assertions are checks, not 52 independent UAT scenarios. See docs/WBS-3.4.2.1-Completion-2026-11-10.md for scope and handoff.

The record above describes Thành's original run before the portability fix. The independent harness now runs 45 checks; it omits the historical order lookup and repeated checks against ID24. The current count is printed by the runner and must not be relabelled as the historical 52-check run. Browser UAT and public deployment remain separate gates.
