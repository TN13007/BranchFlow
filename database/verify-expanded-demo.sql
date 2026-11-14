-- Chỉ đọc/kiểm tra dữ liệu demo. Không reset hoặc sửa dữ liệu.
SET NOCOUNT ON;
DECLARE @tag nvarchar(100)=N'BF-DEMO-EXPANDED-v1';
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test') THROW 51061,N'Chọn đúng DB Dev/Test.',1;
IF (SELECT COUNT(*) FROM dbo.AuditLog WHERE EntityName=@tag AND EntityId=0)<>1 THROW 51061,N'Thiếu marker seed.',1;
IF (SELECT COUNT(*) FROM dbo.SalesOrder o JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@tag)<>180 THROW 51061,N'Thiếu đơn seed.',1;
IF EXISTS(SELECT 1 FROM dbo.SalesOrder o JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@tag
 WHERE o.TotalAmount<>(SELECT SUM(SubtotalAmount) FROM dbo.OrderItem WHERE SalesOrderId=o.Id)
 OR o.BusinessDate<>CONVERT(date,DATEADD(hour,7,o.CreatedAt))) THROW 51061,N'Lệch tổng đơn/ngày nghiệp vụ.',1;
IF EXISTS(SELECT 1 FROM dbo.OrderItem i JOIN dbo.AuditLog a ON a.EntityId=i.SalesOrderId AND a.EntityName=@tag
 OUTER APPLY(SELECT SUM(Quantity*UnitPriceSnapshot) Amount FROM dbo.OrderItemTopping WHERE OrderItemId=i.Id)t
 WHERE i.SubtotalAmount<>i.Quantity*(i.UnitPriceSnapshot+COALESCE(t.Amount,0))) THROW 51061,N'Lệch tiền dòng/topping.',1;
IF EXISTS(SELECT 1 FROM dbo.OrderAdjustment h JOIN dbo.AuditLog a ON a.EntityId=h.SalesOrderId AND a.EntityName=@tag
 WHERE ISJSON(h.BeforeData)<>1 OR ISJSON(h.AfterData)<>1 OR LEN(LTRIM(RTRIM(h.Reason)))=0) THROW 51061,N'Lịch sử thiếu snapshot/lý do.',1;
IF EXISTS(SELECT 1 FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId
 OUTER APPLY(SELECT TOP(1) d.QuantityAfter FROM dbo.StockTransactionDetail d JOIN dbo.StockTransaction t ON t.Id=d.StockTransactionId
 WHERE t.BranchId=bi.BranchId AND d.IngredientId=bi.IngredientId ORDER BY t.Id DESC,d.Id DESC) lastTx
 WHERE i.Name LIKE N'Demo - %' AND (lastTx.QuantityAfter IS NULL OR bi.Quantity<>lastTx.QuantityAfter)) THROW 51061,N'Tồn kho không khớp giao dịch cuối.',1;
IF EXISTS(SELECT 1 FROM dbo.StockTransactionDetail d JOIN dbo.Ingredient i ON i.Id=d.IngredientId
 WHERE i.Name LIKE N'Demo - %' AND (d.QuantityAfter<>d.QuantityBefore+d.QuantityChange OR d.QuantityAfter<0)) THROW 51061,N'Lệch lịch sử kho.',1;
SELECT b.Code,o.Status,COUNT(*) Orders,SUM(o.TotalAmount) Amount FROM dbo.SalesOrder o
 JOIN dbo.AuditLog a ON a.EntityId=o.Id AND a.EntityName=@tag JOIN dbo.Branch b ON b.Id=o.BranchId GROUP BY b.Code,o.Status ORDER BY b.Code,o.Status;
SELECT COUNT(*) SeedOrderLines FROM dbo.OrderItem i JOIN dbo.AuditLog a ON a.EntityId=i.SalesOrderId AND a.EntityName=@tag;
SELECT COUNT(*) SeedAdjustments FROM dbo.OrderAdjustment h JOIN dbo.AuditLog a ON a.EntityId=h.SalesOrderId AND a.EntityName=@tag;
SELECT b.Code,COUNT(*) Balances,SUM(CASE WHEN bi.Quantity=0 THEN 1 ELSE 0 END) OutOfStock,
 SUM(CASE WHEN bi.Quantity>0 AND bi.Quantity<bi.WarningThreshold THEN 1 ELSE 0 END) LowStock
 FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId JOIN dbo.Branch b ON b.Id=bi.BranchId
 WHERE i.Name LIKE N'Demo - %' GROUP BY b.Code ORDER BY b.Code;
SELECT COUNT(*) SeedStockTransactions FROM dbo.AuditLog WHERE EntityName=N'StockTransaction' AND JSON_VALUE(AfterData,'$.Seed')=@tag;
SELECT COUNT(*) AccountsUnchangedCount FROM dbo.AppUser;
PRINT N'PASS: tổng tiền, topping, ngày UTC+7, JSON lịch sử và tồn kho/giao dịch hợp lệ.';
