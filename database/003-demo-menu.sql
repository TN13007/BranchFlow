-- Chỉ dùng cho demo local. Chạy sau 001, 002 và công cụ seed tài khoản.
-- Không sửa giá hoặc trạng thái bản ghi đã tồn tại; không chứa mật khẩu.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET XACT_ABORT ON;
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test')
 THROW 51010,N'Chỉ chạy menu demo trong DB local Dev/Test.',1;
IF NOT EXISTS (SELECT 1 FROM dbo.Branch WHERE Code=N'BF-DEMO-01')
 THROW 51010,N'Chạy công cụ BranchFlow.DemoSeed trước.',1;
BEGIN TRANSACTION;
DECLARE @category INT, @toppingGroup INT, @groupId INT;
IF NOT EXISTS(SELECT 1 FROM dbo.Category WHERE Name=N'Menu demo BranchFlow')
 INSERT dbo.Category(Name,IsActive) VALUES(N'Menu demo BranchFlow',1);
SELECT @category=Id FROM dbo.Category WHERE Name=N'Menu demo BranchFlow' AND Deleted=0 AND IsActive=1;
IF @category IS NULL THROW 51010,N'Danh mục demo bị tắt/xóa; kiểm tra thủ công.',1;
DECLARE @products TABLE(Name NVARCHAR(150),ImageUrl NVARCHAR(500),Price DECIMAL(18,2));
INSERT @products VALUES
 (N'Trà đào cam sả',N'/images/menu/tea.png',30000),
 (N'Trà vải',N'/images/menu/lychee.png',42000),
 (N'Cà phê sữa đá',N'/images/menu/coffee.png',32000),
 (N'Bạc xỉu',N'/images/menu/milk-coffee.png',38000),
 (N'Matcha latte',N'/images/menu/matcha.png',52000);
INSERT dbo.Product(CategoryId,Name,ImageUrl,IsActive)
 SELECT @category,p.Name,p.ImageUrl,1 FROM @products p
 WHERE NOT EXISTS(SELECT 1 FROM dbo.Product x WHERE x.CategoryId=@category AND x.Name=p.Name);
INSERT dbo.ProductSize(ProductId,SizeId,Price,IsActive)
 SELECT p.Id,s.Id,d.Price+CASE WHEN s.Name=N'L' THEN 8000 ELSE 0 END,1
 FROM dbo.Product p JOIN @products d ON d.Name=p.Name CROSS JOIN dbo.Size s
 WHERE p.CategoryId=@category AND s.Name IN(N'M',N'L') AND s.IsActive=1 AND s.Deleted=0
 AND NOT EXISTS(SELECT 1 FROM dbo.ProductSize x WHERE x.ProductId=p.Id AND x.SizeId=s.Id);
INSERT dbo.BranchProduct(BranchId,ProductId,IsAvailable)
 SELECT b.Id,p.Id,1 FROM dbo.Branch b CROSS JOIN dbo.Product p
 WHERE b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND p.CategoryId=@category
 AND NOT EXISTS(SELECT 1 FROM dbo.BranchProduct x WHERE x.BranchId=b.Id AND x.ProductId=p.Id);
-- Món tạm ngưng để đối chiếu SQL với menu trả về; vẫn giữ bản ghi lịch sử.
IF NOT EXISTS(SELECT 1 FROM dbo.Product WHERE CategoryId=@category AND Name=N'Món tạm ngưng (demo)')
 INSERT dbo.Product(CategoryId,Name,IsActive) VALUES(@category,N'Món tạm ngưng (demo)',1);
INSERT dbo.ProductSize(ProductId,SizeId,Price,IsActive)
 SELECT p.Id,s.Id,25000,1 FROM dbo.Product p CROSS JOIN dbo.Size s
 WHERE p.CategoryId=@category AND p.Name=N'Món tạm ngưng (demo)' AND s.Name=N'M'
 AND NOT EXISTS(SELECT 1 FROM dbo.ProductSize x WHERE x.ProductId=p.Id AND x.SizeId=s.Id);
INSERT dbo.BranchProduct(BranchId,ProductId,IsAvailable)
 SELECT b.Id,p.Id,0 FROM dbo.Branch b CROSS JOIN dbo.Product p
 WHERE b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND p.CategoryId=@category AND p.Name=N'Món tạm ngưng (demo)'
 AND NOT EXISTS(SELECT 1 FROM dbo.BranchProduct x WHERE x.BranchId=b.Id AND x.ProductId=p.Id);
IF NOT EXISTS(SELECT 1 FROM dbo.ToppingGroup WHERE Name=N'Topping demo BranchFlow')
 INSERT dbo.ToppingGroup(Name,IsActive) VALUES(N'Topping demo BranchFlow',1);
SELECT @toppingGroup=Id FROM dbo.ToppingGroup WHERE Name=N'Topping demo BranchFlow' AND IsActive=1 AND Deleted=0;
IF @toppingGroup IS NULL THROW 51010,N'Nhóm topping demo không hoạt động.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.Topping WHERE ToppingGroupId=@toppingGroup AND Name=N'Trân châu')
 INSERT dbo.Topping(ToppingGroupId,Name,Price,IsActive) VALUES(@toppingGroup,N'Trân châu',5000,1);
IF NOT EXISTS(SELECT 1 FROM dbo.Topping WHERE ToppingGroupId=@toppingGroup AND Name=N'Thạch')
 INSERT dbo.Topping(ToppingGroupId,Name,Price,IsActive) VALUES(@toppingGroup,N'Thạch',4000,1);
INSERT dbo.BranchTopping(BranchId,ToppingId,IsAvailable)
 SELECT b.Id,t.Id,1 FROM dbo.Branch b CROSS JOIN dbo.Topping t
 WHERE b.Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND t.ToppingGroupId=@toppingGroup
 AND NOT EXISTS(SELECT 1 FROM dbo.BranchTopping x WHERE x.BranchId=b.Id AND x.ToppingId=t.Id);
IF NOT EXISTS(SELECT 1 FROM dbo.NoteGroup WHERE Name=N'Đường (demo)')
 INSERT dbo.NoteGroup(Name,IsActive) VALUES(N'Đường (demo)',1);
IF NOT EXISTS(SELECT 1 FROM dbo.NoteGroup WHERE Name=N'Đá (demo)')
 INSERT dbo.NoteGroup(Name,IsActive) VALUES(N'Đá (demo)',1);
DECLARE @notes TABLE(GroupName NVARCHAR(100),Name NVARCHAR(100));
INSERT @notes VALUES(N'Đường (demo)',N'Ít đường'),(N'Đường (demo)',N'Không đường'),(N'Đá (demo)',N'Ít đá'),(N'Đá (demo)',N'Không đá');
INSERT dbo.NoteOption(NoteGroupId,Name,IsActive)
 SELECT g.Id,n.Name,1 FROM @notes n JOIN dbo.NoteGroup g ON g.Name=n.GroupName
 WHERE NOT EXISTS(SELECT 1 FROM dbo.NoteOption x WHERE x.NoteGroupId=g.Id AND x.Name=n.Name);
COMMIT;
SELECT N'Menu demo sẵn sàng' AS Result,COUNT(*) AS ProductSizes FROM dbo.ProductSize ps JOIN dbo.Product p ON p.Id=ps.ProductId WHERE p.CategoryId=@category;
