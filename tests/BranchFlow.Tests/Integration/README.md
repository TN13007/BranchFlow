# Local SQL integration tests

Run against a local `BranchFlow_Test` database with the baseline/reference data and `BranchFlow.DemoSeed` applied. The tests resolve demo branches and users by code/username; SQL identity values are not fixed. Tests that create transactions clean their own fixtures or roll back.

```powershell
$env:BRANCHFLOW_TEST_CONNECTION_STRING = 'Server=localhost\SQLEXPRESS;Database=BranchFlow_Test;Trusted_Connection=True;TrustServerCertificate=True'
dotnet test BranchFlow.slnx -c Release
Remove-Item Env:BRANCHFLOW_TEST_CONNECTION_STRING
```

Without the environment variable, SQL integration tests are skipped. Do not point the variable to Dev or a shared database.
