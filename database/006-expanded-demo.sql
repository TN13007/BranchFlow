-- Dữ liệu GIẢ LẬP phục vụ trình diễn, không phải lịch sử kinh doanh/UAT thực tế.
-- Nguồn menu: bộ menu cá nhân đã được chuyển đổi trong 004; không nhập lại khách hàng/giao dịch cá nhân.
-- Chạy SSMS: mặc định rollback. Đổi @Apply thành 1 để nạp vào DB local của nhóm.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
DECLARE @Apply bit = 0; -- APPLY_MODE
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test')
    THROW 51060,N'Chỉ dùng DB local Dev/Test của nhóm.',1;
IF CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')) <> CONVERT(nvarchar(128),HOST_NAME())
    THROW 51060,N'Chỉ chạy trực tiếp trên SQL Server cùng máy.',1;
DECLARE @tag nvarchar(100)=N'BF-DEMO-EXPANDED-v1', @actor int, @created bit=0,
    @today date=CONVERT(date,DATEADD(hour,7,SYSUTCDATETIME())), @now datetime2=SYSUTCDATETIME();
SELECT @actor=Id FROM dbo.AppUser WHERE Username=N'demo.systemadmin' AND IsActive=1 AND Deleted=0;
IF @actor IS NULL THROW 51060,N'Cần DemoSeed và tài khoản demo.systemadmin đang hoạt động.',1;
DECLARE @accountHash varbinary(32)=HASHBYTES('SHA2_256',(SELECT * FROM dbo.AppUser ORDER BY Id FOR JSON PATH));
BEGIN TRY
 BEGIN TRANSACTION;
 DECLARE @lock int;
 EXEC @lock=sys.sp_getapplock @Resource=@tag,@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
 IF @lock<0 THROW 51060,N'Không lấy được khóa seed.',1;
 IF EXISTS(SELECT 1 FROM dbo.AuditLog WHERE EntityName=@tag AND EntityId=0)
 BEGIN
    PRINT N'Bộ demo đã tồn tại: không thêm trùng, không reset thay đổi do người demo thực hiện.';
 END
 ELSE
 BEGIN
    SET @created=1;
    DECLARE @branches table(N int PRIMARY KEY,Id int,Creator int);
    INSERT @branches
    SELECT ROW_NUMBER() OVER(ORDER BY b.Code),b.Id,u.Id
    FROM dbo.Branch b
    LEFT JOIN dbo.AppUser u ON u.Username=CASE b.Code WHEN N'BF-DEMO-01' THEN N'demo.employee.a' WHEN N'BF-DEMO-02' THEN N'demo.employee.b' END
       AND u.IsActive=1 AND u.Deleted=0
    WHERE b.Code IN (N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND b.IsActive=1 AND b.Deleted=0;
    IF (SELECT COUNT(*) FROM @branches)<>3 OR (SELECT SUM(CASE WHEN Creator IS NOT NULL THEN 1 ELSE 0 END) FROM @branches)<>2
        THROW 51060,N'Cần ba chi nhánh và Employee A/B có sẵn; không tự tạo mật khẩu hoặc phân công.',1;
    IF EXISTS(SELECT 1 FROM @branches b WHERE b.Creator IS NOT NULL AND NOT EXISTS(
        SELECT 1 FROM dbo.EmployeeBranchAssignment a JOIN dbo.EmployeeProfile e ON e.Id=a.EmployeeProfileId
        WHERE e.UserId=b.Creator AND e.Deleted=0 AND a.BranchId=b.Id AND a.EffectiveFrom<=@now AND (a.EffectiveTo IS NULL OR a.EffectiveTo>@now)))
        THROW 51060,N'Phân công Employee A/B đã đổi; kiểm tra trước khi seed.',1;

    -- Không sửa menu, giá hoặc trạng thái khả dụng hiện có.
    DECLARE @menu table(N int PRIMARY KEY,BranchId int,ProductSizeId int,ProductName nvarchar(150),SizeName nvarchar(50),Price decimal(18,2));
    INSERT @menu
    SELECT ROW_NUMBER() OVER(ORDER BY b.Id,p.Id,ps.Id),b.Id,ps.Id,p.Name,s.Name,ps.Price
    FROM @branches b JOIN dbo.BranchProduct bp ON bp.BranchId=b.Id AND bp.IsAvailable=1
    JOIN dbo.Product p ON p.Id=bp.ProductId AND p.IsActive=1 AND p.Deleted=0
    JOIN dbo.ProductSize ps ON ps.ProductId=p.Id AND ps.IsActive=1 AND ps.Deleted=0
    JOIN dbo.Size s ON s.Id=ps.SizeId AND s.IsActive=1 AND s.Deleted=0
    WHERE b.Creator IS NOT NULL AND ps.Price>0;
    IF (SELECT COUNT(DISTINCT BranchId) FROM @menu)<>2 THROW 51060,N'Thiếu menu mở bán ở chi nhánh 01/02.',1;

    -- 12 nguyên liệu giả lập; tên riêng tránh đụng hàng QA/đang dùng.
    DECLARE @ingredients table(N int PRIMARY KEY,Name nvarchar(150),UnitName nvarchar(50),Id int);
    INSERT @ingredients(N,Name,UnitName) VALUES
      (1,N'Demo - Trà lài',N'kg'),(2,N'Demo - Trà đen',N'kg'),(3,N'Demo - Trà ô long',N'kg'),
      (4,N'Demo - Bột matcha',N'kg'),(5,N'Demo - Cà phê rang xay',N'kg'),(6,N'Demo - Bột sữa',N'kg'),
      (7,N'Demo - Đường cát',N'kg'),(8,N'Demo - Trân châu đen',N'kg'),(9,N'Demo - Thạch dừa',N'kg'),
      (10,N'Demo - Sữa tươi',N'lít'),(11,N'Demo - Syrup đào',N'lít'),(12,N'Demo - Syrup vải',N'lít');
    IF EXISTS(SELECT 1 FROM @ingredients i JOIN dbo.Ingredient x ON x.Name=i.Name)
       THROW 51060,N'Nguyên liệu trùng tên nhưng thiếu marker. Không ghi đè; cần kiểm tra thủ công.',1;
    IF EXISTS(SELECT 1 FROM @ingredients i WHERE NOT EXISTS(SELECT 1 FROM dbo.IngredientUnit u WHERE u.Name=i.UnitName AND u.Deleted=0 AND u.IsActive=1))
       THROW 51060,N'Thiếu đơn vị reference kg/lít.',1;
    INSERT dbo.Ingredient(UnitId,Name,IsActive,CreatedAt,Deleted)
    SELECT u.Id,i.Name,1,@now,0 FROM @ingredients i JOIN dbo.IngredientUnit u ON u.Name=i.UnitName AND u.Deleted=0 AND u.IsActive=1;
    UPDATE i SET Id=x.Id FROM @ingredients i JOIN dbo.Ingredient x ON x.Name=i.Name;
    INSERT dbo.BranchIngredient(BranchId,IngredientId,Quantity,WarningThreshold,UpdatedAt)
    SELECT b.Id,i.Id,0,CASE WHEN i.N<=3 THEN 8 ELSE 5 END,@now FROM @branches b CROSS JOIN @ingredients i;
    DECLARE @bn int=1,@branch int,@step int,@tx int,@at datetime2;
    WHILE @bn<=3
    BEGIN
       SELECT @branch=Id FROM @branches WHERE N=@bn;
       SET @step=1;
       WHILE @step<=4
       BEGIN
          SET @at=DATEADD(day,-(5-@step),@now);
          INSERT dbo.StockTransaction(BranchId,PerformedByUserId,Type,CreatedAt)
          VALUES(@branch,@actor,CASE WHEN @step IN(1,3) THEN N'IN' ELSE N'OUT' END,@at);
          SET @tx=SCOPE_IDENTITY();
          INSERT dbo.StockTransactionDetail(StockTransactionId,IngredientId,QuantityChange,QuantityBefore,QuantityAfter)
          SELECT @tx,i.Id,d.Q,bi.Quantity,bi.Quantity+d.Q
          FROM @ingredients i JOIN dbo.BranchIngredient bi ON bi.IngredientId=i.Id AND bi.BranchId=@branch
          CROSS APPLY(SELECT CONVERT(decimal(18,3),CASE @step WHEN 1 THEN 20+@bn*5+i.N WHEN 2 THEN -(4+i.N%3) WHEN 3 THEN 10
             ELSE CASE WHEN i.N=1 THEN -bi.Quantity WHEN i.N IN(2,3) THEN 3-bi.Quantity ELSE -(6+i.N%4) END END) Q)d;
          UPDATE bi SET Quantity=d.QuantityAfter,UpdatedAt=@at FROM dbo.BranchIngredient bi
          JOIN dbo.StockTransactionDetail d ON d.IngredientId=bi.IngredientId AND d.StockTransactionId=@tx WHERE bi.BranchId=@branch;
          INSERT dbo.AuditLog(BranchId,EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
          VALUES(@branch,N'StockTransaction',@tx,N'CREATE',
            (SELECT @tag AS Seed,N'Dữ liệu nhập/xuất giả lập phục vụ demo; không phải hoạt động thực tế.' AS Reason FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),@actor,@at);
          SET @step+=1;
       END;
       SET @bn+=1;
    END;

    -- 180 đơn / 30 ngày tới ngày seed (UTC+7), 90 đơn mỗi CN01/02; CN03 để demo trạng thái chưa có doanh thu.
    DECLARE @n int=0,@creator int,@date date,@code nvarchar(50),@seq int,@order int,@line int,
      @ps int,@pn nvarchar(150),@sn nvarchar(50),@price decimal(18,2),@cups int,@lines int,@li int,
      @top int,@topname nvarchar(150),@topprice decimal(18,2),@topqty int,
      @before nvarchar(max),@after nvarchar(max);
    WHILE @n<180
    BEGIN
       SET @bn=1+@n%2;
       SELECT @branch=Id,@creator=Creator FROM @branches WHERE N=@bn;
       SET @date=DATEADD(day,-29+@n/6,@today);
       SET @at=DATEADD(hour,-7,DATEADD(minute,(@n%6)*23+15,DATEADD(hour,9,CONVERT(datetime2,@date))));
       -- Ngày hiện tại dùng thời điểm đã qua để không tạo giao dịch tương lai.
       IF @date=@today AND @at>@now SET @at=@now;
       EXEC dbo.usp_GetNextOrderCode @BranchId=@branch,@BusinessDate=@date,@OrderCode=@code OUTPUT,@SequenceNumber=@seq OUTPUT;
       INSERT dbo.SalesOrder(Code,BranchId,CreatedByUserId,BusinessDate,DailySequence,TotalAmount,Status,CreatedAt)
       VALUES(@code,@branch,@creator,@date,@seq,0,N'COMPLETED',@at);
       SET @order=SCOPE_IDENTITY(); SET @li=1; SET @lines=CASE WHEN @n%3=0 THEN 2 ELSE 1 END;
       WHILE @li<=@lines
       BEGIN
          SELECT @ps=ProductSizeId,@pn=ProductName,@sn=SizeName,@price=Price FROM (
             SELECT *,ROW_NUMBER() OVER(ORDER BY N) R,COUNT(*) OVER() C FROM @menu WHERE BranchId=@branch
          )m WHERE R=1+(@n*7+@li*11)%C;
          SET @cups=1+(@n+@li)%3;
          SET @top=NULL; SET @topqty=0; SET @topprice=0;
          IF @n%3<>0
             SELECT TOP(1) @top=t.Id,@topname=t.Name,@topprice=t.Price,@topqty=1+@n%3
             FROM dbo.Topping t JOIN dbo.BranchTopping bt ON bt.ToppingId=t.Id AND bt.BranchId=@branch AND bt.IsAvailable=1
             WHERE t.IsActive=1 AND t.Deleted=0 AND t.Name NOT LIKE N'%1300%'
             ORDER BY (t.Id+@n)%17,t.Id;
          INSERT dbo.OrderItem(SalesOrderId,ProductSizeId,ProductNameSnapshot,SizeNameSnapshot,Quantity,UnitPriceSnapshot,SubtotalAmount)
          VALUES(@order,@ps,@pn,@sn,@cups,@price,@cups*(@price+@topprice*@topqty));
          SET @line=SCOPE_IDENTITY();
          IF @top IS NOT NULL INSERT dbo.OrderItemTopping(OrderItemId,ToppingId,ToppingNameSnapshot,Quantity,UnitPriceSnapshot)
             VALUES(@line,@top,@topname,@topqty,@topprice);
          INSERT dbo.OrderItemNote(OrderItemId,NoteOptionId,NoteNameSnapshot)
          SELECT @line,Id,Name FROM (
             SELECT o.*,ROW_NUMBER() OVER(PARTITION BY o.NoteGroupId ORDER BY (o.Id+@n)%7,o.Id) R
             FROM dbo.NoteOption o JOIN dbo.NoteGroup g ON g.Id=o.NoteGroupId
             WHERE o.IsActive=1 AND o.Deleted=0 AND g.IsActive=1 AND g.Deleted=0
          ) q WHERE R=1;
          SET @li+=1;
       END;
       UPDATE dbo.SalesOrder SET TotalAmount=(SELECT SUM(SubtotalAmount) FROM dbo.OrderItem WHERE SalesOrderId=@order) WHERE Id=@order;
       IF @n%20 IN(0,1,10,11)
       BEGIN
          SELECT @before=(SELECT o.Id,o.Code,o.Status,o.TotalAmount,
            JSON_QUERY((SELECT i.Id,i.ProductSizeId,i.ProductNameSnapshot,i.SizeNameSnapshot,i.UnitPriceSnapshot,i.Quantity,i.SubtotalAmount,
              JSON_QUERY((SELECT ToppingId,ToppingNameSnapshot,Quantity,UnitPriceSnapshot FROM dbo.OrderItemTopping WHERE OrderItemId=i.Id FOR JSON PATH)) Toppings,
              JSON_QUERY((SELECT NoteOptionId,NoteNameSnapshot FROM dbo.OrderItemNote WHERE OrderItemId=i.Id FOR JSON PATH)) Notes
              FROM dbo.OrderItem i WHERE i.SalesOrderId=o.Id ORDER BY i.Id FOR JSON PATH)) Items
            FROM dbo.SalesOrder o WHERE o.Id=@order FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
          IF @n%20 IN(0,1) UPDATE dbo.SalesOrder SET Status=N'CANCELLED',UpdatedAt=@at WHERE Id=@order;
          ELSE
          BEGIN
             UPDATE dbo.OrderItem SET SubtotalAmount=SubtotalAmount/Quantity*(Quantity+1),Quantity=Quantity+1 WHERE Id=@line;
             UPDATE dbo.SalesOrder SET TotalAmount=(SELECT SUM(SubtotalAmount) FROM dbo.OrderItem WHERE SalesOrderId=@order),UpdatedAt=@at WHERE Id=@order;
          END;
          SELECT @after=(SELECT o.Id,o.Code,o.Status,o.TotalAmount,
            JSON_QUERY((SELECT i.Id,i.ProductSizeId,i.ProductNameSnapshot,i.SizeNameSnapshot,i.UnitPriceSnapshot,i.Quantity,i.SubtotalAmount,
              JSON_QUERY((SELECT ToppingId,ToppingNameSnapshot,Quantity,UnitPriceSnapshot FROM dbo.OrderItemTopping WHERE OrderItemId=i.Id FOR JSON PATH)) Toppings,
              JSON_QUERY((SELECT NoteOptionId,NoteNameSnapshot FROM dbo.OrderItemNote WHERE OrderItemId=i.Id FOR JSON PATH)) Notes
              FROM dbo.OrderItem i WHERE i.SalesOrderId=o.Id ORDER BY i.Id FOR JSON PATH)) Items
            FROM dbo.SalesOrder o WHERE o.Id=@order FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
          INSERT dbo.OrderAdjustment(SalesOrderId,AdjustedByUserId,Reason,BeforeData,AfterData,CreatedAt)
          VALUES(@order,@actor,CASE WHEN @n%20 IN(0,1) THEN N'Dữ liệu demo: khách đổi ý, hủy đơn giả lập.' ELSE N'Dữ liệu demo: điều chỉnh số ly giả lập theo yêu cầu khách.' END,@before,@after,@at);
       END;
       INSERT dbo.AuditLog(BranchId,EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
       VALUES(@branch,@tag,@order,N'CREATE',(SELECT @n AS CaseIndex,N'Dữ liệu tổng hợp phục vụ demo, không phải kết quả UAT.' AS Source FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),@actor,@now);
       SET @n+=1;
    END;
    INSERT dbo.AuditLog(EntityName,EntityId,Action,AfterData,PerformedByUserId,CreatedAt)
    VALUES(@tag,0,N'CREATE',(SELECT @today AS AnchorDate,180 AS SeedOrders,12 AS SeedIngredients,N'Synthetic demo v1' AS Source FOR JSON PATH,WITHOUT_ARRAY_WRAPPER),@actor,@now);
 END;
 IF @accountHash<>HASHBYTES('SHA2_256',(SELECT * FROM dbo.AppUser ORDER BY Id FOR JSON PATH)) THROW 51060,N'Tài khoản thay đổi ngoài dự kiến.',1;
 IF (SELECT COUNT(*) FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId WHERE a.EntityName=@tag AND a.EntityId>0)<>180
    THROW 51060,N'Marker/đơn demo thiếu; không tự tạo lại hoặc ghi đè.',1;
 IF EXISTS(SELECT 1 FROM dbo.OrderItem i JOIN dbo.AuditLog a ON a.EntityId=i.SalesOrderId AND a.EntityName=@tag
    OUTER APPLY(SELECT SUM(t.Quantity*t.UnitPriceSnapshot) Amount FROM dbo.OrderItemTopping t WHERE t.OrderItemId=i.Id)t
    WHERE i.SubtotalAmount<>i.Quantity*(i.UnitPriceSnapshot+COALESCE(t.Amount,0))) THROW 51060,N'Tổng dòng demo không khớp snapshot.',1;
 IF EXISTS(SELECT 1 FROM dbo.SalesOrder o JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@tag
    WHERE o.TotalAmount<>(SELECT SUM(SubtotalAmount) FROM dbo.OrderItem WHERE SalesOrderId=o.Id)
       OR o.BusinessDate<>CONVERT(date,DATEADD(hour,7,o.CreatedAt))) THROW 51060,N'Tổng đơn/ngày UTC+7 không khớp.',1;
 SELECT b.Code,o.Status,COUNT(*) Orders,SUM(o.TotalAmount) Amount,MIN(o.BusinessDate) FromDate,MAX(o.BusinessDate) ToDate
 FROM dbo.SalesOrder o JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@tag JOIN dbo.Branch b ON b.Id=o.BranchId GROUP BY b.Code,o.Status ORDER BY b.Code,o.Status;
 SELECT COUNT(*) SeedIngredients FROM dbo.Ingredient WHERE Name LIKE N'Demo - %';
 IF @Apply=1 BEGIN COMMIT; IF @created=1 PRINT N'APPLIED: dữ liệu demo đã lưu, tài khoản và dữ liệu cũ được giữ nguyên.'; ELSE PRINT N'UNCHANGED: không tạo thêm hoặc ghi đè dữ liệu demo đã tồn tại.'; END
 ELSE BEGIN ROLLBACK; PRINT N'PREVIEW: đã rollback toàn bộ dữ liệu/counter trong lượt thử.'; END;
END TRY
BEGIN CATCH
 IF @@TRANCOUNT>0 ROLLBACK;
 THROW;
END CATCH;
