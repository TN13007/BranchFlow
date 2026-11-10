SET NOCOUNT ON;
IF DB_NAME()<>N'BranchFlow_Test' THROW 51020,'Use local BranchFlow_Test only.',1;
DECLARE @order int=(SELECT Id FROM dbo.SalesOrder WHERE Code=N'WBS3421-e5356f5b170f');
IF @order IS NULL THROW 51020,'Completion fixture missing.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SalesOrder WHERE Id=@order AND Status=N'CANCELLED' AND TotalAmount=196000)
 THROW 51020,'Completion header mismatch.',1;
IF (SELECT COUNT(*) FROM dbo.OrderAdjustment WHERE SalesOrderId=@order)<>3
 THROW 51020,'Rejected or rolled-back requests created extra history.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderItem WHERE SalesOrderId=@order AND Quantity=4 AND SizeNameSnapshot=N'M'
 AND UnitPriceSnapshot=30000 AND SubtotalAmount=196000 AND Note=N'WBS3421 rollback note')
 THROW 51020,'Completion detail mismatch.',1;
IF (SELECT COUNT(*) FROM dbo.OrderItemTopping t JOIN dbo.OrderItem i ON i.Id=t.OrderItemId WHERE i.SalesOrderId=@order
 AND ((t.ToppingNameSnapshot=N'Trân châu' AND t.Quantity=3 AND t.UnitPriceSnapshot=5000)
 OR (t.ToppingNameSnapshot=N'Thạch' AND t.Quantity=1 AND t.UnitPriceSnapshot=4000)))<>2
 THROW 51020,'Completion topping snapshot mismatch.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderItemNote n JOIN dbo.OrderItem i ON i.Id=n.OrderItemId WHERE i.SalesOrderId=@order)
 THROW 51020,'Structured note missing.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.OrderAdjustment WHERE SalesOrderId=@order AND Reason=N'WBS3421 quantity'
 AND JSON_VALUE(BeforeData,'$.TotalAmount')='98000.00' AND JSON_VALUE(AfterData,'$.TotalAmount')='147000.00')
BEGIN
 IF NOT EXISTS(SELECT 1 FROM dbo.OrderAdjustment WHERE SalesOrderId=@order AND Reason=N'WBS3421 quantity'
 AND TRY_CONVERT(decimal(18,2),JSON_VALUE(BeforeData,'$.TotalAmount'))=98000
 AND TRY_CONVERT(decimal(18,2),JSON_VALUE(AfterData,'$.TotalAmount'))=147000)
 THROW 51020,'First adjustment history mismatch.',1;
END;
IF NOT EXISTS(SELECT 1 FROM dbo.SalesOrder WHERE Code=N'BF-DEMO-01-20261110-0009' AND Status=N'CANCELLED' AND TotalAmount=147000)
 OR (SELECT COUNT(*) FROM dbo.OrderAdjustment a JOIN dbo.SalesOrder o ON o.Id=a.SalesOrderId WHERE o.Code=N'BF-DEMO-01-20261110-0009')<>2
 THROW 51020,'Original browser evidence changed.',1;
PRINT 'PASS retained completion fixture: cancelled196000, quantity4, size/toppings/notes preserved, exactly3 history';
PRINT 'PASS original browser evidence remains cancelled147000 with exactly2 history';
SELECT o.Id,o.Code,o.Status,o.TotalAmount FROM dbo.SalesOrder o WHERE o.Id=@order;
SELECT a.Reason,u.Username AS PerformedBy,JSON_VALUE(a.BeforeData,'$.TotalAmount') AS BeforeTotal,
 JSON_VALUE(a.AfterData,'$.TotalAmount') AS AfterTotal,JSON_VALUE(a.AfterData,'$.Status') AS AfterStatus
 FROM dbo.OrderAdjustment a JOIN dbo.AppUser u ON u.Id=a.AdjustedByUserId WHERE a.SalesOrderId=@order ORDER BY a.Id;
PRINT 'This read-only script checks retained state; the HTTP/rollback execution is recorded in Completion-HTTP-SQL-2026-11-10.txt.';
