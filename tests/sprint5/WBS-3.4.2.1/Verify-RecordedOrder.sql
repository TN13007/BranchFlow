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
IF EXISTS(SELECT 1 FROM dbo.SalesOrder WHERE Id=@order AND Status=N'COMPLETED')
 THROW 51020,'Cancelled recorded order unexpectedly included in completed-only revenue.',1;
PRINT 'PASS recorded order is excluded from completed-only revenue; no order was deleted';
SELECT o.Id,o.Code,b.Code AS BranchCode,o.Status,o.TotalAmount
 FROM dbo.SalesOrder o JOIN dbo.Branch b ON b.Id=o.BranchId WHERE o.Id=@order;
SELECT a.Reason,u.Username AS PerformedBy,a.CreatedAt
 FROM dbo.OrderAdjustment a JOIN dbo.AppUser u ON u.Id=a.AdjustedByUserId WHERE a.SalesOrderId=@order ORDER BY a.Id;
PRINT 'Read-only replay of stored evidence completed; this does not rerun UI, denied HTTP requests, stale update or forced rollback.';