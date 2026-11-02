-- Chỉ kiểm tra trên DB Test; mọi fixture được rollback.
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT OFF;
IF DB_NAME()<>N'BranchFlow_Test' THROW 51000,N'Chỉ được chạy kiểm thử trong BranchFlow_Test.',1;
GO
-- Xác nhận SQL thật từ chối dữ liệu sai, không chỉ kiểm tra biểu thức trong code.
CREATE PROCEDURE #ExpectFailure @Name NVARCHAR(100), @Sql NVARCHAR(MAX), @Expected INT
AS
BEGIN
    DECLARE @Actual INT=0;
    BEGIN TRY
        EXEC sys.sp_executesql @Sql;
    END TRY
    BEGIN CATCH
        SET @Actual=ERROR_NUMBER();
    END CATCH;
    IF @Actual<>@Expected
    BEGIN
        DECLARE @Message NVARCHAR(2048)=CONCAT(@Name,N': expected ',@Expected,N', actual ',@Actual);
        THROW 51010,@Message,1;
    END;
    PRINT CONCAT(N'PASS: ',@Name);
END;
GO
IF DB_NAME()<>N'BranchFlow_Test' THROW 51000,N'Chỉ được chạy kiểm thử trong BranchFlow_Test.',1;
BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @EmployeeRole INT=(SELECT Id FROM dbo.[Role] WHERE Code=N'EMPLOYEE' AND Deleted=0);
    DECLARE @AdminRole INT=(SELECT Id FROM dbo.[Role] WHERE Code=N'BRANCH_ADMIN' AND Deleted=0);
    -- Đây là fixture constraint, không phải tài khoản đăng nhập hoặc hash mật khẩu thật.
    INSERT dbo.AppUser(RoleId,Username,PasswordHash,FullName)
    VALUES (@EmployeeRole,N'__verify_employee',N'CONSTRAINT_TEST_ONLY_NOT_A_LOGIN_HASH',N'Fixture'),
           (@AdminRole,N'__verify_admin',N'CONSTRAINT_TEST_ONLY_NOT_A_LOGIN_HASH',N'Fixture');
    DECLARE @User INT=(SELECT Id FROM dbo.AppUser WHERE Username=N'__verify_employee');
    DECLARE @Admin INT=(SELECT Id FROM dbo.AppUser WHERE Username=N'__verify_admin');
    INSERT dbo.EmployeeProfile(UserId,EmployeeCode,HireDate) VALUES(@User,N'__verify_emp','20260101');
    DECLARE @Employee INT=SCOPE_IDENTITY();
    INSERT dbo.Branch(Code,Name,Address) VALUES(N'__verify_branch',N'Fixture',N'Local');
    DECLARE @Branch INT=SCOPE_IDENTITY();
    INSERT dbo.Branch(Code,Name,Address) VALUES(N'__verify_branch2',N'Fixture2',N'Local');
    DECLARE @Branch2 INT=SCOPE_IDENTITY();
    INSERT dbo.AdminBranchAssignment(AdminUserId,BranchId,EffectiveFrom,AssignedByUserId)
    VALUES(@Admin,@Branch,SYSUTCDATETIME(),@Admin);
    INSERT dbo.EmployeeBranchAssignment(EmployeeProfileId,BranchId,EffectiveFrom,AssignedByUserId)
    VALUES(@Employee,@Branch,SYSUTCDATETIME(),@Admin);
    DECLARE @Sql NVARCHAR(MAX);
    SET @Sql=CONCAT(N'INSERT dbo.AdminBranchAssignment(AdminUserId,BranchId,EffectiveFrom,AssignedByUserId) VALUES(',@Admin,',',@Branch,',SYSUTCDATETIME(),',@Admin,')');
    EXEC #ExpectFailure N'One open admin per branch',@Sql,2601;
    SET @Sql=CONCAT(N'INSERT dbo.EmployeeBranchAssignment(EmployeeProfileId,BranchId,EffectiveFrom,AssignedByUserId) VALUES(',@Employee,',',@Branch2,',SYSUTCDATETIME(),',@Admin,')');
    EXEC #ExpectFailure N'One open branch per employee',@Sql,2601;
    UPDATE dbo.EmployeeProfile SET Deleted=1 WHERE Id=@Employee;
    SET @Sql=CONCAT(N'INSERT dbo.EmployeeProfile(UserId,EmployeeCode,HireDate) VALUES(',@User,',N''__verify_emp2'',''20260101'')');
    EXEC #ExpectFailure N'Employee user remains unique after soft delete',@Sql,2601;
    UPDATE dbo.EmployeeProfile SET Deleted=0 WHERE Id=@Employee;
    SET @Sql=CONCAT(N'INSERT dbo.EmployeeBranchAssignment(EmployeeProfileId,BranchId,EffectiveFrom,EffectiveTo,AssignedByUserId) VALUES(',@Employee,',',@Branch,',''20260102'',''20260101'',',@Admin,')');
    EXEC #ExpectFailure N'Invalid assignment time range',@Sql,547;
    EXEC #ExpectFailure N'Foreign key enforced',N'INSERT dbo.Product(CategoryId,Name) VALUES(-1,N''Invalid FK'')',547;

    INSERT dbo.Category(Name) VALUES(N'__verify_category');
    DECLARE @Category INT=SCOPE_IDENTITY();
    INSERT dbo.Product(CategoryId,Name) VALUES(@Category,N'__verify_product');
    DECLARE @Product INT=SCOPE_IDENTITY();
    DECLARE @Size INT=(SELECT Id FROM dbo.Size WHERE Name=N'M' AND Deleted=0);
    INSERT dbo.ProductSize(ProductId,SizeId,Price) VALUES(@Product,@Size,30000);
    DECLARE @ProductSize INT=SCOPE_IDENTITY();
    SET @Sql=CONCAT(N'INSERT dbo.ProductSize(ProductId,SizeId,Price) VALUES(',@Product,',',@Size,',30000)');
    EXEC #ExpectFailure N'Active product-size unique',@Sql,2601;
    SET @Sql=CONCAT(N'UPDATE dbo.ProductSize SET Price=-1 WHERE Id=',@ProductSize);
    EXEC #ExpectFailure N'Negative price rejected',@Sql,547;
    INSERT dbo.ToppingGroup(Name) VALUES(N'__verify_group');
    DECLARE @Group INT=SCOPE_IDENTITY();
    INSERT dbo.Topping(ToppingGroupId,Name,Price) VALUES(@Group,N'__verify_topping',5000);
    DECLARE @Topping INT=SCOPE_IDENTITY();
    INSERT dbo.SalesOrder(Code,BranchId,CreatedByUserId,BusinessDate,DailySequence,TotalAmount)
    VALUES(N'__verify_order',@Branch,@User,'20261102',1,98000);
    DECLARE @Order INT=SCOPE_IDENTITY();
    INSERT dbo.OrderItem(SalesOrderId,ProductSizeId,ProductNameSnapshot,SizeNameSnapshot,Quantity,UnitPriceSnapshot,SubtotalAmount)
    VALUES(@Order,@ProductSize,N'Fixture',N'M',2,30000,98000);
    DECLARE @Item INT=SCOPE_IDENTITY();
    INSERT dbo.OrderItemTopping(OrderItemId,ToppingId,ToppingNameSnapshot,Quantity,UnitPriceSnapshot)
    VALUES(@Item,@Topping,N'Fixture',3,5000);
    SET @Sql=CONCAT(N'UPDATE dbo.OrderItemTopping SET Quantity=4 WHERE OrderItemId=',@Item);
    EXEC #ExpectFailure N'Topping four portions rejected',@Sql,547;
    SET @Sql=CONCAT(N'UPDATE dbo.OrderItemTopping SET Quantity=0 WHERE OrderItemId=',@Item);
    EXEC #ExpectFailure N'Topping zero portions rejected',@Sql,547;
    SET @Sql=CONCAT(N'INSERT dbo.OrderItemTopping(OrderItemId,ToppingId,ToppingNameSnapshot,Quantity) VALUES(',@Item,',',@Topping,',N''Duplicate'',1)');
    EXEC #ExpectFailure N'Duplicate topping rejected',@Sql,2601;
    SET @Sql=CONCAT(N'DELETE dbo.ProductSize WHERE Id=',@ProductSize);
    EXEC #ExpectFailure N'History is protected, no cascade delete',@Sql,547;
    SET @Sql=CONCAT(N'UPDATE dbo.SalesOrder SET Status=N''ADJUSTED'' WHERE Id=',@Order);
    EXEC #ExpectFailure N'Legacy adjusted status rejected',@Sql,547;
    SET @Sql=CONCAT(N'INSERT dbo.SalesOrder(Code,BranchId,CreatedByUserId,BusinessDate,DailySequence) VALUES(N''__verify_other'',',@Branch,',',@User,',''20261102'',1)');
    EXEC #ExpectFailure N'Branch/date/sequence unique',@Sql,2601;
    DECLARE @Version BINARY(8)=(SELECT RowVersion FROM dbo.SalesOrder WHERE Id=@Order);
    UPDATE dbo.SalesOrder SET TotalAmount=TotalAmount+1 WHERE Id=@Order;
    UPDATE dbo.SalesOrder SET TotalAmount=TotalAmount+1 WHERE Id=@Order AND RowVersion=@Version;
    IF @@ROWCOUNT<>0 THROW 51010,N'Stale RowVersion update should affect zero rows.',1;
    PRINT N'PASS: stale RowVersion update prevented';
    INSERT dbo.Payroll(EmployeeProfileId,BranchId,[Year],[Month],BaseSalarySnapshot,AllowanceAmount,BonusAmount,DeductionAmount,CreatedByUserId,UpdatedByUserId)
    VALUES(@Employee,@Branch,2026,11,1000000,100000,50000,20000,@Admin,@Admin);
    DECLARE @Payroll INT=SCOPE_IDENTITY();
    IF (SELECT TotalSalary FROM dbo.Payroll WHERE Id=@Payroll)<>1130000 THROW 51010,N'Payroll computed total mismatch.',1;
    PRINT N'PASS: payroll computed total';
    SET @Sql=CONCAT(N'UPDATE dbo.Payroll SET DeductionAmount=2000000 WHERE Id=',@Payroll);
    EXEC #ExpectFailure N'Negative payroll total rejected',@Sql,547;
    DECLARE @Unit INT=(SELECT TOP(1) Id FROM dbo.IngredientUnit ORDER BY Id);
    INSERT dbo.Ingredient(UnitId,Name) VALUES(@Unit,N'__verify_ingredient');
    DECLARE @Ingredient INT=SCOPE_IDENTITY();
    INSERT dbo.BranchIngredient(BranchId,IngredientId,Quantity) VALUES(@Branch,@Ingredient,10);
    DECLARE @Inventory INT=SCOPE_IDENTITY();
    SET @Sql=CONCAT(N'UPDATE dbo.BranchIngredient SET Quantity=-1 WHERE Id=',@Inventory);
    EXEC #ExpectFailure N'Negative inventory rejected',@Sql,547;
    INSERT dbo.Stocktake(Code,BranchId,CheckedByUserId) VALUES(N'__verify_stocktake',@Branch,@User);
    DECLARE @Stocktake INT=SCOPE_IDENTITY();
    INSERT dbo.StocktakeItem(StocktakeId,BranchIngredientId,SystemQuantity,ActualQuantity,InventoryRowVersionSnapshot)
    SELECT @Stocktake,Id,Quantity,12.125,RowVersion FROM dbo.BranchIngredient WHERE Id=@Inventory;
    IF (SELECT Difference FROM dbo.StocktakeItem WHERE StocktakeId=@Stocktake)<>2.125 THROW 51010,N'Difference precision mismatch.',1;
    PRINT N'PASS: stocktake computed decimal(18,3)';
    DECLARE @Session UNIQUEIDENTIFIER=NEWID();
    INSERT dbo.AuthSession(Id,UserId,ExpiresAt) VALUES(@Session,@User,DATEADD(DAY,7,SYSUTCDATETIME()));
    INSERT dbo.RefreshToken(Id,SessionId,TokenHash,ExpiresAt) VALUES(NEWID(),@Session,REPLICATE('a',64),DATEADD(DAY,7,SYSUTCDATETIME()));
    SET @Sql=CONCAT(N'INSERT dbo.RefreshToken(Id,SessionId,TokenHash,ExpiresAt) VALUES(NEWID(),''',@Session,N''',REPLICATE(''a'',64),DATEADD(DAY,7,SYSUTCDATETIME()))');
    EXEC #ExpectFailure N'Refresh hash unique',@Sql,2601;

    -- Procedure phải reset biến OUTPUT, không dùng lại sequence của lời gọi trước.
    DECLARE @Code NVARCHAR(50),@Seq INT=999;
    EXEC dbo.usp_GetNextOrderCode @Branch,'20261103',@Code OUTPUT,@Seq OUTPUT;
    IF @Seq<>1 THROW 51010,N'Counter did not reset supplied output.',1;
    EXEC dbo.usp_GetNextOrderCode @Branch,'20261103',@Code OUTPUT,@Seq OUTPUT;
    IF @Seq<>2 THROW 51010,N'Counter sequence mismatch.',1;
    UPDATE dbo.OrderDailyCounter SET LastNumber=9999 WHERE BranchId=@Branch AND BusinessDate='20261103';
    EXEC dbo.usp_GetNextOrderCode @Branch,'20261103',@Code OUTPUT,@Seq OUTPUT;
    IF @Seq<>10000 OR RIGHT(@Code,6)<>N'-10000' THROW 51010,N'Counter truncated after 9999.',1;
    PRINT N'PASS: counter output reset, sequential allocation and 10000 boundary';
    ROLLBACK TRANSACTION;
    IF EXISTS(SELECT 1 FROM dbo.Branch WHERE Code=N'__verify_branch')
       OR EXISTS(SELECT 1 FROM dbo.OrderDailyCounter WHERE BranchId=@Branch)
        THROW 51010,N'Rollback left test fixtures.',1;
    PRINT N'PASS: fixture/order/counter rollback; no test data retained';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
