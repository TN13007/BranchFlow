-- Read-only verification of the recorded 10/11/2026 local browser journey.
-- This verifies preserved evidence data; it does not replay browser actions.
SET NOCOUNT ON;
IF DB_NAME()<>N'BranchFlow_Test' THROW 51020,'Use BranchFlow_Test only.',1;
DECLARE @order int=(SELECT Id FROM dbo.SalesOrder WHERE Code=N'BF-DEMO-01-20261110-0009');
IF @order IS NULL THROW 51020,'Recorded UAT order missing.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SalesOrder o JOIN dbo.AppUser u ON u.Id=o.CreatedByUserId
 WHERE o.Id=@order AND u.Username=N'demo.employee.a' AND o.Status=N'CANCELLED' AND o.TotalAmount=147000)
 THROW 51020,'Order final state/creator/total mismatch.',1;
PRINT 'PASS UAT order final state CANCELLED, creator Employee A, total 147000';
IF (SELECT COUNT(*) FROM dbo.OrderItem WHERE SalesOrderId=@order AND Quantity=3 AND UnitPriceSnapshot=30000 AND SubtotalAmount=147000)<>1
 THROW 51020,'Item snapshot mismatch.',1;
IF (SELECT COUNT(*) FROM dbo.OrderItemTopping t JOIN dbo.OrderItem i ON i.Id=t.OrderItemId
 WHERE i.SalesOrderId=@order AND ((t.ToppingNameSnapshot=N'Trân châu' AND t.Quantity=3 AND t.UnitPriceSnapshot=5000)
 OR (t.ToppingNameSnapshot=N'Thạch' AND t.Quantity=1 AND t.UnitPriceSnapshot=4000)))<>2
 THROW 51020,'Topping snapshot mismatch.',1;
PRINT 'PASS item and topping snapshots match three cups at 49000 each';
IF (SELECT COUNT(*) FROM dbo.OrderAdjustment WHERE SalesOrderId=@order)<>2
 THROW 51020,'Expected exactly two adjustments.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderAdjustment WHERE SalesOrderId=@order AND Reason=N'UAT5-20261110 adjust 2 to 3'
 AND TRY_CONVERT(decimal(18,2),JSON_VALUE(BeforeData,'$.TotalAmount'))=98000
 AND TRY_CONVERT(decimal(18,2),JSON_VALUE(AfterData,'$.TotalAmount'))=147000
 AND JSON_VALUE(AfterData,'$.Status')=N'COMPLETED') THROW 51020,'Adjustment audit mismatch.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderAdjustment WHERE SalesOrderId=@order AND Reason=N'UAT5-20261110 cancel'
 AND JSON_VALUE(BeforeData,'$.Status')=N'COMPLETED' AND JSON_VALUE(AfterData,'$.Status')=N'CANCELLED')
 THROW 51020,'Cancellation audit mismatch.',1;
PRINT 'PASS exactly two history records: adjust then cancel; invalid empty reason did not add history';
IF (SELECT COUNT(*) FROM dbo.AuditLog a JOIN dbo.StockTransaction t ON t.Id=a.EntityId
 WHERE a.EntityName=N'StockTransaction' AND JSON_VALUE(a.AfterData,'$.Reason') LIKE N'UAT5-20261110 stock %')<>2
 THROW 51020,'Expected exactly two committed UAT stock transactions.',1;
IF (SELECT COUNT(*) FROM dbo.StockTransactionDetail d JOIN dbo.AuditLog a ON a.EntityId=d.StockTransactionId AND a.EntityName=N'StockTransaction'
 WHERE (JSON_VALUE(a.AfterData,'$.Reason')=N'UAT5-20261110 stock IN' AND d.QuantityBefore=7 AND d.QuantityChange=2 AND d.QuantityAfter=9)
 OR (JSON_VALUE(a.AfterData,'$.Reason')=N'UAT5-20261110 stock OUT' AND d.QuantityBefore=9 AND d.QuantityChange=-2 AND d.QuantityAfter=7))<>2
 THROW 51020,'Stock before/change/after mismatch.',1;
PRINT 'PASS stock 7 -> 9 -> 7; failed OUT 10 did not commit a third UAT transaction';
IF NOT EXISTS(SELECT 1 FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId
 JOIN dbo.Branch b ON b.Id=bi.BranchId WHERE i.Name=N'FE-0d68e065 stock' AND b.Code=N'BF-DEMO-01' AND bi.Quantity=7)
 THROW 51020,'Final stock mismatch.',1;
IF (SELECT COUNT(*) FROM dbo.BranchIngredient bi JOIN dbo.Ingredient i ON i.Id=bi.IngredientId JOIN dbo.Branch b ON b.Id=bi.BranchId
 WHERE i.Name=N'WBS5 - Cốt trà đào' AND bi.Quantity=CASE b.Code WHEN N'BF-DEMO-01' THEN 7 WHEN N'BF-DEMO-02' THEN 15 WHEN N'BF-DEMO-03' THEN 4 END)<>3
 THROW 51020,'WBS demo balances changed.',1;
PRINT 'PASS stock restored; selling/adjusting/cancelling did not consume WBS ingredient';
SELECT o.Id,o.Code,o.Status,o.TotalAmount FROM dbo.SalesOrder o WHERE o.Id=@order;
SELECT b.Code,COUNT(*) AS CompletedOrders,SUM(o.TotalAmount) AS Revenue
 FROM dbo.SalesOrder o JOIN dbo.Branch b ON b.Id=o.BranchId
 WHERE o.Status=N'COMPLETED' AND CONVERT(date,DATEADD(hour,7,o.CreatedAt))='20261110' GROUP BY b.Code;
IF (SELECT COUNT(*) FROM dbo.SalesOrder WHERE Code IN (N'BF-DEMO-01-20261110-0010',N'BF-DEMO-01-20261110-0011',N'BF-DEMO-01-20261110-0012') AND Status=N'COMPLETED' AND TotalAmount=29000)<>3
 THROW 51020,'Retest retained order mismatch.',1;
PRINT 'PASS retained U25/U27/U28 orders25/26/27 exist once per code, completed29000 each';
PRINT 'Read-only evidence recheck; does not replay browser, double click or post-fault events.';
