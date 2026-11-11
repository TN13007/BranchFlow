-- Read-only manifest checks. Never seed/reset from this verification script.
SET NOCOUNT ON;
IF DB_NAME()<>N'BranchFlow_Test' THROW 51012,'Use local BranchFlow_Test.',1;
DECLARE @category int=(SELECT Id FROM dbo.Category WHERE Name=N'Menu demo BranchFlow' AND Deleted=0);
DECLARE @expected table(Name nvarchar(150),Price decimal(18,2));
INSERT @expected VALUES(N'Trà đào cam sả',30000),(N'Trà vải',42000),(N'Cà phê sữa đá',32000),(N'Bạc xỉu',38000),(N'Matcha latte',52000);
IF (SELECT COUNT(*) FROM @expected e JOIN dbo.Product p ON p.Name=e.Name AND p.CategoryId=@category AND p.Deleted=0 AND p.IsActive=1
 JOIN dbo.ProductSize ps ON ps.ProductId=p.Id AND ps.Deleted=0 AND ps.IsActive=1 JOIN dbo.Size s ON s.Id=ps.SizeId
 WHERE (s.Name=N'M' AND ps.Price=e.Price) OR (s.Name=N'L' AND ps.Price=e.Price+8000))<>10
 THROW 51012,'Five demo drinks must each have expected M/L prices.',1;
IF (SELECT COUNT(*) FROM @expected e JOIN dbo.Product p ON p.Name=e.Name AND p.CategoryId=@category AND p.Deleted=0
 JOIN dbo.BranchProduct bp ON bp.ProductId=p.Id JOIN dbo.Branch b ON b.Id=bp.BranchId
 WHERE b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND bp.IsAvailable=1)<>15
 THROW 51012,'Expected five demo drinks available at each demo branch.',1;
IF (SELECT COUNT(*) FROM dbo.Product p JOIN dbo.BranchProduct bp ON bp.ProductId=p.Id JOIN dbo.Branch b ON b.Id=bp.BranchId
 WHERE p.CategoryId=@category AND p.Name=N'Món tạm ngưng (demo)' AND p.Deleted=0 AND bp.IsAvailable=0
 AND b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03'))<>3
 THROW 51012,'Stopped drink must remain stopped at all demo branches.',1;
PRINT 'PASS five active demo drinks, ten M/L prices, three-branch availability and one stopped demo drink';
DECLARE @cases table(CaseName nvarchar(30),State nvarchar(20),Cups int,Total decimal(18,2));
INSERT @cases VALUES(N'completed-a',N'COMPLETED',2,98000),(N'cancelled-a',N'CANCELLED',3,147000),(N'review-a',N'NEEDS_REVIEW',1,49000),(N'completed-b',N'COMPLETED',1,30000);
IF (SELECT COUNT(*) FROM dbo.AuditLog WHERE EntityName=N'BF-WBS5-v1' AND EntityId<>0)<>4
 THROW 51012,'Expected exactly four tagged order fixtures.',1;
IF (SELECT COUNT(*) FROM @cases e JOIN dbo.AuditLog a ON a.EntityName=N'BF-WBS5-v1' AND JSON_VALUE(a.AfterData,'$.Case')=e.CaseName
 JOIN dbo.SalesOrder o ON o.Id=a.EntityId JOIN dbo.OrderItem i ON i.SalesOrderId=o.Id
 WHERE o.Status=e.State AND o.TotalAmount=e.Total AND i.Quantity=e.Cups AND i.UnitPriceSnapshot=30000 AND i.SizeNameSnapshot=N'M')<>4
 THROW 51012,'Fixture state, cups, price or total mismatch.',1;
IF EXISTS(SELECT 1 FROM dbo.AuditLog a JOIN dbo.OrderItem i ON i.SalesOrderId=a.EntityId
 OUTER APPLY(SELECT SUM(t.Quantity*t.UnitPriceSnapshot) Amount FROM dbo.OrderItemTopping t WHERE t.OrderItemId=i.Id) x
 WHERE a.EntityName=N'BF-WBS5-v1' AND i.SubtotalAmount<>i.Quantity*(i.UnitPriceSnapshot+COALESCE(x.Amount,0)))
 THROW 51012,'Snapshot arithmetic mismatch.',1;
IF (SELECT SUM(o.TotalAmount) FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId
 WHERE a.EntityName=N'BF-WBS5-v1' AND o.Status=N'COMPLETED')<>128000
 THROW 51012,'Fixture-only completed revenue mismatch.',1;
IF EXISTS(SELECT 1 FROM dbo.AuditLog a JOIN dbo.OrderAdjustment h ON h.SalesOrderId=a.EntityId WHERE a.EntityName=N'BF-WBS5-v1')
 THROW 51012,'Static fixtures must not be presented as recorded UI adjustments.',1;
PRINT 'PASS four static fixtures, expected states/totals/cups/snapshots; fixture completed revenue128000';
SELECT p.Name,s.Name AS Size,ps.Price,b.Code,bp.IsAvailable FROM dbo.Product p JOIN dbo.ProductSize ps ON ps.ProductId=p.Id
 JOIN dbo.Size s ON s.Id=ps.SizeId JOIN dbo.BranchProduct bp ON bp.ProductId=p.Id JOIN dbo.Branch b ON b.Id=bp.BranchId
 WHERE p.CategoryId=@category AND p.Deleted=0 AND ps.Deleted=0 AND b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') ORDER BY p.Name,s.Name,b.Code;
SELECT JSON_VALUE(a.AfterData,'$.Case') AS DemoCase,o.Id,o.Code,b.Code AS Branch,o.Status,i.Quantity,o.TotalAmount,o.BusinessDate
 FROM dbo.AuditLog a JOIN dbo.SalesOrder o ON o.Id=a.EntityId JOIN dbo.Branch b ON b.Id=o.BranchId JOIN dbo.OrderItem i ON i.SalesOrderId=o.Id
 WHERE a.EntityName=N'BF-WBS5-v1' ORDER BY o.Id;
