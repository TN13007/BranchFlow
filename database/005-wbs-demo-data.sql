-- Fixture local cho WBS 5.1.1 / 5.1.2 / 5.1.3. Không đổi schema hoặc dữ liệu cũ.
-- Trạng thái đơn được seed trực tiếp để demo; không phải kết quả UAT luồng chuyển trạng thái.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test')
    THROW 51010,N'Chỉ dùng database local Dev/Test.',1;

DECLARE @fixture nvarchar(100)=N'BF-WBS5-v1', @actor int, @employeeA int, @employeeB int;
SELECT @actor=Id FROM dbo.AppUser WHERE Username=N'demo.systemadmin' AND IsActive=1 AND Deleted=0;
SELECT @employeeA=Id FROM dbo.AppUser WHERE Username=N'demo.employee.a' AND IsActive=1 AND Deleted=0;
SELECT @employeeB=Id FROM dbo.AppUser WHERE Username=N'demo.employee.b' AND IsActive=1 AND Deleted=0;
IF @actor IS NULL OR @employeeA IS NULL OR @employeeB IS NULL
    THROW 51010,N'Chạy DemoSeed trước; không tự đổi tài khoản hiện có.',1;
DECLARE @branches table(Id int PRIMARY KEY, Code nvarchar(30), InQty decimal(18,3), OutQty decimal(18,3));
INSERT @branches SELECT Id,Code,
    CASE Code WHEN N'BF-DEMO-01' THEN 10 WHEN N'BF-DEMO-02' THEN 20 ELSE 5 END,
    CASE Code WHEN N'BF-DEMO-01' THEN 3 WHEN N'BF-DEMO-02' THEN 5 ELSE 1 END
FROM dbo.Branch WHERE Code IN (N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND IsActive=1 AND Deleted=0;
IF (SELECT COUNT(*) FROM @branches)<>3 THROW 51010,N'Cần đủ 3 chi nhánh demo đang hoạt động.',1;
DECLARE @cases table(CaseName nvarchar(30), BranchCode nvarchar(30), State nvarchar(20), Cups int, WithTopping bit, Total decimal(18,2));
INSERT @cases VALUES
    (N'completed-a',N'BF-DEMO-01',N'COMPLETED',2,1,98000),
    (N'cancelled-a',N'BF-DEMO-01',N'CANCELLED',3,1,147000),
    (N'review-a',N'BF-DEMO-01',N'NEEDS_REVIEW',1,1,49000),
    (N'completed-b',N'BF-DEMO-02',N'COMPLETED',1,0,30000);

BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'BranchFlow-BF-WBS5-v1',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51010,N'Không khóa được lượt seed demo.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.AuditLog WHERE EntityName=@fixture AND EntityId=0)
BEGIN
    DECLARE @now datetime2(7)=SYSUTCDATETIME(), @businessDate date=CONVERT(date,DATEADD(hour,7,SYSUTCDATETIME()));
    DECLARE @productSize int,@productName nvarchar(150),@sizeName nvarchar(50),@price decimal(18,2),@pearl int,@jelly int;
    SELECT @productSize=ps.Id,@productName=p.Name,@sizeName=s.Name,@price=ps.Price
    FROM dbo.ProductSize ps JOIN dbo.Product p ON p.Id=ps.ProductId JOIN dbo.Size s ON s.Id=ps.SizeId
    JOIN dbo.Category c ON c.Id=p.CategoryId
    WHERE c.Name=N'Menu demo BranchFlow' AND p.Name=N'Trà đào cam sả' AND s.Name=N'M'
      AND p.IsActive=1 AND p.Deleted=0 AND ps.IsActive=1 AND ps.Deleted=0 AND s.IsActive=1 AND s.Deleted=0;
    SELECT @pearl=t.Id FROM dbo.Topping t JOIN dbo.ToppingGroup g ON g.Id=t.ToppingGroupId
    WHERE g.Name=N'Topping demo BranchFlow' AND t.Name=N'Trân châu' AND t.Price=5000 AND t.IsActive=1 AND t.Deleted=0;
    SELECT @jelly=t.Id FROM dbo.Topping t JOIN dbo.ToppingGroup g ON g.Id=t.ToppingGroupId
    WHERE g.Name=N'Topping demo BranchFlow' AND t.Name=N'Thạch' AND t.Price=4000 AND t.IsActive=1 AND t.Deleted=0;
    IF @productSize IS NULL OR @price<>30000 OR @pearl IS NULL OR @jelly IS NULL
        THROW 51010,N'Menu demo khác fixture 30000/5000/4000; kiểm tra thủ công, không ghi đè giá.',1;

    DECLARE @case nvarchar(30),@branchCode nvarchar(30),@status nvarchar(20),@cups int,@hasTopping bit,@total decimal(18,2);
    DECLARE order_cases CURSOR LOCAL FAST_FORWARD FOR SELECT CaseName,BranchCode,State,Cups,WithTopping,Total FROM @cases;
    OPEN order_cases;
    FETCH NEXT FROM order_cases INTO @case,@branchCode,@status,@cups,@hasTopping,@total;
    WHILE @@FETCH_STATUS=0
    BEGIN
        DECLARE @branch int=(SELECT Id FROM @branches WHERE Code=@branchCode),@code nvarchar(50),@seq int,@order int,@line int,@creator int;
        SET @creator=CASE WHEN @branchCode=N'BF-DEMO-01' THEN @employeeA ELSE @employeeB END;
        EXEC dbo.usp_GetNextOrderCode @BranchId=@branch,@BusinessDate=@businessDate,@OrderCode=@code OUTPUT,@SequenceNumber=@seq OUTPUT;
        INSERT dbo.SalesOrder(Code,BranchId,CreatedByUserId,BusinessDate,DailySequence,TotalAmount,Status,CreatedAt,ReportReason,ReportedByUserId,ReportedAt)
        VALUES(@code,@branch,@creator,@businessDate,@seq,@total,@status,@now,
            CASE WHEN @status=N'NEEDS_REVIEW' THEN N'Đơn fixture cần xem xét, không phải yêu cầu báo sai thực tế.' END,
            CASE WHEN @status=N'NEEDS_REVIEW' THEN @creator END,CASE WHEN @status=N'NEEDS_REVIEW' THEN @now END);
        SET @order=SCOPE_IDENTITY();
        INSERT dbo.OrderItem(SalesOrderId,ProductSizeId,ProductNameSnapshot,SizeNameSnapshot,Quantity,UnitPriceSnapshot,SubtotalAmount)
        VALUES(@order,@productSize,@productName,@sizeName,@cups,@price,@total);
        SET @line=SCOPE_IDENTITY();
        IF @hasTopping=1
            INSERT dbo.OrderItemTopping(OrderItemId,ToppingId,ToppingNameSnapshot,Quantity,UnitPriceSnapshot)
            VALUES(@line,@pearl,N'Trân châu',3,5000),(@line,@jelly,N'Thạch',1,4000);
        DECLARE @json nvarchar(max)=(SELECT @case AS [Case],@code AS Code,@status AS Status,@total AS ExpectedTotal FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
        INSERT dbo.AuditLog(BranchId,EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
        VALUES(@branch,@fixture,@order,N'CREATE',@json,@actor,@now);
        FETCH NEXT FROM order_cases INTO @case,@branchCode,@status,@cups,@hasTopping,@total;
    END;
    CLOSE order_cases; DEALLOCATE order_cases;

    DECLARE @unit int=(SELECT Id FROM dbo.IngredientUnit WHERE Name=N'lít' AND IsActive=1 AND Deleted=0),@ingredient int;
    IF @unit IS NULL THROW 51010,N'Cần đơn vị lít từ reference data.',1;
    IF EXISTS(SELECT 1 FROM dbo.Ingredient WHERE Name=N'WBS5 - Cốt trà đào')
        THROW 51010,N'Tên nguyên liệu fixture đã tồn tại nhưng chưa có marker; không ghi đè.',1;
    INSERT dbo.Ingredient(UnitId,Name,IsActive,CreatedAt,Deleted) VALUES(@unit,N'WBS5 - Cốt trà đào',1,@now,0);
    SET @ingredient=SCOPE_IDENTITY();
    DECLARE @stockBranch int,@in decimal(18,3),@out decimal(18,3),@tx int;
    DECLARE stock_cases CURSOR LOCAL FAST_FORWARD FOR SELECT Id,InQty,OutQty FROM @branches;
    OPEN stock_cases; FETCH NEXT FROM stock_cases INTO @stockBranch,@in,@out;
    WHILE @@FETCH_STATUS=0
    BEGIN
        INSERT dbo.BranchIngredient(BranchId,IngredientId,Quantity,WarningThreshold,UpdatedAt) VALUES(@stockBranch,@ingredient,@in-@out,8,@now);
        INSERT dbo.StockTransaction(BranchId,PerformedByUserId,Type,CreatedAt) VALUES(@stockBranch,@actor,N'IN',@now);
        SET @tx=SCOPE_IDENTITY();
        INSERT dbo.StockTransactionDetail(StockTransactionId,IngredientId,QuantityChange,QuantityBefore,QuantityAfter) VALUES(@tx,@ingredient,@in,0,@in);
        INSERT dbo.AuditLog(BranchId,EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
        VALUES(@stockBranch,N'StockTransaction',@tx,N'CREATE',N'{"Reason":"BF-WBS5-v1 demo inbound"}',@actor,@now);
        INSERT dbo.StockTransaction(BranchId,PerformedByUserId,Type,CreatedAt) VALUES(@stockBranch,@actor,N'OUT',DATEADD(second,1,@now));
        SET @tx=SCOPE_IDENTITY();
        INSERT dbo.StockTransactionDetail(StockTransactionId,IngredientId,QuantityChange,QuantityBefore,QuantityAfter) VALUES(@tx,@ingredient,-@out,@in,@in-@out);
        INSERT dbo.AuditLog(BranchId,EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
        VALUES(@stockBranch,N'StockTransaction',@tx,N'CREATE',N'{"Reason":"BF-WBS5-v1 demo outbound"}',@actor,DATEADD(second,1,@now));
        FETCH NEXT FROM stock_cases INTO @stockBranch,@in,@out;
    END;
    CLOSE stock_cases; DEALLOCATE stock_cases;
    INSERT dbo.AuditLog(EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
    VALUES(@fixture,0,N'CREATE',N'{"Fixture":"BF-WBS5-v1","Orders":4,"StockTransactions":6,"ExpectedRevenue":128000}',@actor,@now);
END;

-- Kiểm chứng chạy cả ở lần đầu và khi chạy lại. Không tự sửa fixture bị người dùng thay đổi.
IF (SELECT COUNT(*) FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId JOIN @cases c ON c.CaseName=JSON_VALUE(a.AfterData,'$.Case')
    WHERE a.EntityName=@fixture AND o.Status=c.State AND o.TotalAmount=c.Total)<>4
    THROW 51010,N'Đơn fixture không khớp expected; kiểm tra thủ công.',1;
IF (SELECT COUNT(*) FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId JOIN @branches b ON b.Id=bi.BranchId
    WHERE i.Name=N'WBS5 - Cốt trà đào' AND bi.Quantity=b.InQty-b.OutQty)<>3
    THROW 51010,N'Tồn fixture đã thay đổi; kiểm tra thủ công.',1;
IF (SELECT COUNT(*) FROM dbo.AuditLog a JOIN dbo.StockTransaction t ON t.Id=a.EntityId AND a.EntityName=N'StockTransaction'
    WHERE JSON_VALUE(a.AfterData,'$.Reason') IN (N'BF-WBS5-v1 demo inbound',N'BF-WBS5-v1 demo outbound'))<>6
    THROW 51010,N'Lịch sử kho fixture thiếu giao dịch.',1;
IF EXISTS(SELECT 1 FROM dbo.OrderItem i JOIN dbo.AuditLog a ON a.EntityId=i.SalesOrderId AND a.EntityName=@fixture
    OUTER APPLY(SELECT SUM(t.Quantity*t.UnitPriceSnapshot) AS ToppingAmount FROM dbo.OrderItemTopping t WHERE t.OrderItemId=i.Id) x
    WHERE i.SubtotalAmount<>i.Quantity*(i.UnitPriceSnapshot+COALESCE(x.ToppingAmount,0)))
    THROW 51010,N'Snapshot tiền món/topping không khớp.',1;
IF (SELECT SUM(o.TotalAmount) FROM dbo.SalesOrder o JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@fixture WHERE o.Status=N'COMPLETED')<>128000
    THROW 51010,N'Doanh thu fixture không khớp 128000.',1;
COMMIT;
SELECT CONVERT(nvarchar(30),JSON_VALUE(a.AfterData,'$.Case')) AS DemoCase,o.Id,o.Code,b.Code AS BranchCode,o.Status,o.TotalAmount,o.BusinessDate
FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId JOIN dbo.Branch b ON b.Id=o.BranchId WHERE a.EntityName=@fixture ORDER BY o.Id;
SELECT b.Code,i.Name,bi.Quantity,bi.WarningThreshold FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId
JOIN dbo.Branch b ON b.Id=bi.BranchId WHERE i.Name=N'WBS5 - Cốt trà đào' ORDER BY b.Code;
PRINT N'PASS BF-WBS5-v1: 4 orders, 3 branch balances, fixture revenue 128000; rerun does not duplicate.';
