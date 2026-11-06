-- Menu Tuấn Kiệt chuyển từ dữ liệu seed dự án cá nhân sang schema nhóm.
-- Chỉ bổ sung local Dev/Test; không sửa menu cũ, tài khoản, đơn hoặc lịch sử giá.
-- 62 món, 3 dung tích, 9 topping; đã loại UP SIZE 1300ML theo quyết định nhóm.
-- Có thể chạy lại. Nếu trùng tên khác danh mục/nhóm, thêm hậu tố (Tuấn Kiệt).
-- Chạy sau baseline, reference data và công cụ seed tài khoản/chi nhánh demo.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test')
    THROW 51020,N'Chỉ nạp menu vào DB local Dev/Test của nhóm.',1;
IF CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')) <> CONVERT(nvarchar(128),HOST_NAME())
    THROW 51020,N'Chỉ chạy seed trên SQL Server cùng máy.',1;

-- Đổi thành 0 để kiểm tra và rollback, không lưu dữ liệu.
DECLARE @Apply bit = 1;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @Branches TABLE(Id int PRIMARY KEY);
INSERT @Branches SELECT Id FROM dbo.Branch
WHERE Code IN(N'BF-DEMO-01',N'BF-DEMO-02',N'BF-DEMO-03') AND IsActive=1 AND Deleted=0;
IF NOT EXISTS(SELECT 1 FROM @Branches)
    THROW 51020,N'Chưa có chi nhánh demo đang hoạt động; chạy DemoSeed trước.',1;
DECLARE @Menu TABLE(CategoryName nvarchar(150),ProductName nvarchar(150),Price500 decimal(18,2),Price700 decimal(18,2),Price1000 decimal(18,2),ImageUrl nvarchar(1000),TargetName nvarchar(150));
INSERT @Menu(CategoryName,ProductName,Price500,Price700,Price1000,ImageUrl) VALUES
(N'Bestseller!!! & Signature', N'HỒNG TRÀ TẮC ĐÁ XAY', 16000, 20000, 25000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ed2ohv7wl8fc'),
    (N'Bestseller!!! & Signature', N'HỒNG TRÀ ĐÀO ĐÁ XAY', 17000, 24000, 30000, NULL),
    (N'Bestseller!!! & Signature', N'HỒNG TRÀ TẮC THÁI XANH XÍ MUỘI', 17000, 24000, 28000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecv8t8xxh8f8'),
    (N'Bestseller!!! & Signature', N'GIỌT NGỌC DƯỠNG SINH', 25000, 32000, 40000, NULL),
    (N'Bestseller!!! & Signature', N'LONG NHÃN TUẤN KIỆT', 24000, 28000, 34000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edz32m2qld0c'),
    (N'Bestseller!!! & Signature', N'TRÀ ME MUỐI ỚT', 28000, 32000, 40000, NULL),
    (N'Bestseller!!! & Signature', N'ĐÁ ME ĐẬU PHỘNG', 22000, 28000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecb3xppm0le9'),

    (N'Trà !!!', N'TRÀ TRÁI CÂY NHIỆT ĐỚI', 27000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ed2zht4iphfb'),
    (N'Trà !!!', N'TRÀ MÃNG CẦU', 27000, 30000, 40000, NULL),
    (N'Trà !!!', N'TRÀ DƯA LƯỚI', 27000, 30000, 40000, NULL),
    (N'Trà !!!', N'TRÀ THƠM', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ XOÀI', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ ỔI', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ DƯA HẤU', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ CHANH DÂY', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ CHANH DÂY XOÀI', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ VẢI TRÂN CHÂU THẠCH', 27000, 30000, 37000, NULL),
    (N'Trà !!!', N'TRÀ CHANH DÂY CAM XOÀI', 27000, 30000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecfal197lod2'),
    (N'Trà !!!', N'TRÀ GỪNG CAM SẢ', 27000, 30000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ee7a5w8v5o9b'),
    (N'Trà !!!', N'TRÀ CÓC GIÃ TAY', 27000, 30000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edv8977uht4a'),
    (N'Trà !!!', N'TRÀ CHANH TƯƠI', 22000, 27000, 32000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ee5gko888s20'),

    (N'Nước Ép', N'NƯỚC ÉP THƠM', 22000, 32000, 37000, NULL),
    (N'Nước Ép', N'NƯỚC ÉP ỔI', 22000, 32000, 37000, NULL),
    (N'Nước Ép', N'NƯỚC ÉP DƯA HẤU', 22000, 32000, 37000, NULL),
    (N'Nước Ép', N'NƯỚC ÉP CAM', 22000, 32000, 37000, NULL),

    (N'Nước Sâm', N'SÂM BÔNG CÚC', 19000, 24000, 28000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3eccnf6ik6t70'),
    (N'Nước Sâm', N'SÂM RONG BIỂN', 19000, 24000, 28000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecfjh88mqp3c'),
    (N'Nước Sâm', N'SÂM ĐẮNG GIẢI NHIỆT', 19000, 24000, 28000, NULL),
    (N'Nước Sâm', N'SÂM BÍ ĐAO', 19000, 24000, 28000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-820l4-meh3vfp4nm6a23'),
    (N'Nước Sâm', N'SÂM THẢO MỘC 24 VỊ', 21000, 27000, 31000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecvuwgm9bpa6'),

    (N'Matcha', N'MATCHA LATTE', 28000, 37000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ee84x98ek5cb'),
    (N'Matcha', N'MATCHA ĐÁ XAY', 28000, 37000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m4zz3ymvop1460'),
    (N'Matcha', N'MATCHA LATTE DÂU', 30000, 39000, 45000, NULL),
    (N'Matcha', N'MATCHA LATTE VIỆT QUẤT', 30000, 39000, 45000, NULL),
    (N'Matcha', N'MATCHA LATTE ĐÀO', 30000, 39000, 45000, NULL),
    (N'Matcha', N'SỮA CHUA MATCHA', 32000, 37000, 47000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ee8lav9lewd8'),

    (N'Trà Sữa', N'TRÀ SỮA TRUYỀN THỐNG', 20000, 29000, 38000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecwg23nnwc05'),
    (N'Trà Sữa', N'TRÀ SỮA TRÂN CHÂU ĐƯỜNG ĐEN', 20000, 29000, 38000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edbfma45v9c4'),

    (N'Sữa Tươi', N'SÂM DỨA SỮA', 22000, 27000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3xcel0ry1th42'),
    (N'Sữa Tươi', N'SỮA TƯƠI DÂU', 22000, 27000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ed2hyasmnwf3'),
    (N'Sữa Tươi', N'SỮA TƯƠI TRÂN CHÂU ĐƯỜNG ĐEN', 22000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3xcfw2f3mi000'),
    (N'Sữa Tươi', N'SỮA TƯƠI THẠCH ĐÀO', 22000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ecw0iqe8cc3d'),
    (N'Sữa Tươi', N'MILO DẦM', 24000, 32000, 40000, NULL),
    (N'Sữa Tươi', N'CACAO ĐÁ XAY', 24000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ed279qguco15'),
    (N'Sữa Tươi', N'DỪA XAY CACAO', 24000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ed92gkp069b9'),
    (N'Sữa Tươi', N'CHANH DÂY SỮA ĐÁ', 24000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edtvqa8azs50'),

    (N'Sữa Chua', N'SỮA CHUA VIỆT QUẤT', 27000, 32000, 40000, NULL),
    (N'Sữa Chua', N'SỮA CHUA DÂU', 27000, 32000, 40000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ec9jlq5arg46'),
    (N'Sữa Chua', N'SỮA CHUA DƯA LƯỚI', 27000, 32000, 40000, NULL),
    (N'Sữa Chua', N'SỮA CHUA DƯA LƯỚI MIX TRÁI CÂY', 30000, 37000, 45000, NULL),
    (N'Sữa Chua', N'SỮA CHUA VIỆT QUẤT MIX TRÁI CÂY', 30000, 37000, 45000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edu1gfumcoe5'),
    (N'Sữa Chua', N'SỮA CHUA DÂU MIX TRÁI CÂY', 30000, 37000, 45000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3edu70rpf3wc7'),

    (N'Nước Dừa', N'NƯỚC DỪA TẮC', 18000, 22000, 32000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m4cv0r3h9fg021'),
    (N'Nước Dừa', N'NƯỚC DỪA MIX HẠT CHIA', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX TRÂN CHÂU', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX ỔI', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX XOÀI', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX THƠM', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX MÍT', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX CAM', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX DƯA HẤU', 24000, 28000, 37000, NULL),
    (N'Nước Dừa', N'NƯỚC DỪA MIX MATCHA', 24000, 28000, 37000, N'https://down-zl-vn.img.susercontent.com/vn-11134517-7ras8-m3ee8apcrw6gda');
DECLARE @Toppings TABLE(GroupName nvarchar(100),Name nvarchar(150),Price decimal(18,2),TargetName nvarchar(150));
INSERT @Toppings(GroupName,Name,Price) VALUES
(N'Trân châu', N'TRÂN CHÂU ĐEN', 5000),
    (N'Trân châu', N'TRÂN CHÂU TRẮNG', 5000),
    (N'Thạch', N'THẠCH CÁ', 5000),
    (N'Thạch', N'THẠCH TRỨNG', 5000),
    (N'Thạch', N'THẠCH TÁO', 5000),
    (N'Thạch', N'THẠCH TRÁI CÂY', 5000),
    (N'Thạch', N'THẠCH PUDDING', 5000),
    (N'Thạch', N'THẠCH RAU CÂU', 5000),
    (N'Thạch', N'THẠCH CỦ NĂNG', 5000);
IF (SELECT COUNT(*) FROM @Menu)<>62 OR (SELECT COUNT(*) FROM @Toppings)<>9
    THROW 51020,N'Danh sách seed không đủ 62 món và 9 topping.',1;
UPDATE @Menu SET TargetName=ProductName;
UPDATE m SET TargetName=m.ProductName+N' (Tuấn Kiệt)'
FROM @Menu m JOIN dbo.Product p ON p.Name=m.ProductName AND p.Deleted=0
JOIN dbo.Category c ON c.Id=p.CategoryId WHERE c.Name<>m.CategoryName;
UPDATE @Toppings SET TargetName=Name;
UPDATE t SET TargetName=t.Name+N' (Tuấn Kiệt)'
FROM @Toppings t JOIN dbo.Topping p ON p.Name=t.Name AND p.Deleted=0
JOIN dbo.ToppingGroup g ON g.Id=p.ToppingGroupId WHERE g.Name<>t.GroupName;

INSERT dbo.Size(Name)
SELECT v.Name FROM (VALUES(N'500ML'),(N'700ML'),(N'1000ML'))v(Name)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Size s WHERE s.Name=v.Name AND s.Deleted=0);
INSERT dbo.Category(Name)
SELECT DISTINCT m.CategoryName FROM @Menu m
WHERE NOT EXISTS(SELECT 1 FROM dbo.Category c WHERE c.Name=m.CategoryName AND c.Deleted=0);
IF EXISTS(SELECT 1 FROM dbo.Category c JOIN @Menu m ON m.CategoryName=c.Name WHERE c.Deleted=0 AND c.IsActive=0)
 OR EXISTS(SELECT 1 FROM dbo.Size WHERE Name IN(N'500ML',N'700ML',N'1000ML') AND Deleted=0 AND IsActive=0)
    THROW 51020,N'Danh mục hoặc dung tích đã bị tắt; không tự ghi đè trạng thái.',1;
IF EXISTS(SELECT 1 FROM @Menu m JOIN dbo.Product p ON p.Name=m.TargetName AND p.Deleted=0
 JOIN dbo.Category c ON c.Id=p.CategoryId WHERE c.Name<>m.CategoryName OR p.IsActive=0)
    THROW 51020,N'Tên món đích bị trùng hoặc món đã bị tắt; cần kiểm tra trước.',1;
INSERT dbo.Product(CategoryId,Name,ImageUrl)
SELECT c.Id,m.TargetName,m.ImageUrl FROM @Menu m JOIN dbo.Category c ON c.Name=m.CategoryName AND c.Deleted=0
WHERE NOT EXISTS(SELECT 1 FROM dbo.Product p WHERE p.Name=m.TargetName AND p.Deleted=0);
DECLARE @Prices TABLE(ProductId int,SizeId int,Price decimal(18,2),PRIMARY KEY(ProductId,SizeId));
INSERT @Prices
SELECT p.Id,s.Id,v.Price FROM @Menu m
JOIN dbo.Product p ON p.Name=m.TargetName AND p.Deleted=0
CROSS APPLY(VALUES(N'500ML',m.Price500),(N'700ML',m.Price700),(N'1000ML',m.Price1000))v(SizeName,Price)
JOIN dbo.Size s ON s.Name=v.SizeName AND s.Deleted=0;
IF EXISTS(SELECT 1 FROM @Prices x JOIN dbo.ProductSize ps ON ps.ProductId=x.ProductId AND ps.SizeId=x.SizeId AND ps.Deleted=0 WHERE ps.Price<>x.Price OR ps.IsActive=0)
    THROW 51020,N'Giá hoặc trạng thái món đích đã thay đổi; seed không ghi đè giá.',1;
INSERT dbo.ProductSize(ProductId,SizeId,Price)
SELECT x.ProductId,x.SizeId,x.Price FROM @Prices x
WHERE NOT EXISTS(SELECT 1 FROM dbo.ProductSize ps WHERE ps.ProductId=x.ProductId AND ps.SizeId=x.SizeId AND ps.Deleted=0);
INSERT dbo.BranchProduct(BranchId,ProductId,IsAvailable)
SELECT b.Id,p.Id,1 FROM @Branches b CROSS JOIN @Menu m JOIN dbo.Product p ON p.Name=m.TargetName AND p.Deleted=0
WHERE NOT EXISTS(SELECT 1 FROM dbo.BranchProduct bp WHERE bp.BranchId=b.Id AND bp.ProductId=p.Id);

INSERT dbo.ToppingGroup(Name)
SELECT DISTINCT t.GroupName FROM @Toppings t
WHERE NOT EXISTS(SELECT 1 FROM dbo.ToppingGroup g WHERE g.Name=t.GroupName AND g.Deleted=0);
IF EXISTS(SELECT 1 FROM @Toppings t JOIN dbo.ToppingGroup g ON g.Name=t.GroupName AND g.Deleted=0 WHERE g.IsActive=0)
 OR EXISTS(SELECT 1 FROM @Toppings x JOIN dbo.Topping t ON t.Name=x.TargetName AND t.Deleted=0
 JOIN dbo.ToppingGroup g ON g.Id=t.ToppingGroupId WHERE t.Price<>x.Price OR t.IsActive=0 OR g.Name<>x.GroupName)
    THROW 51020,N'Topping đích trùng tên, khác giá hoặc bị tắt; không ghi đè.',1;
INSERT dbo.Topping(ToppingGroupId,Name,Price)
SELECT g.Id,t.TargetName,t.Price FROM @Toppings t JOIN dbo.ToppingGroup g ON g.Name=t.GroupName AND g.Deleted=0
WHERE NOT EXISTS(SELECT 1 FROM dbo.Topping x WHERE x.Name=t.TargetName AND x.Deleted=0);
INSERT dbo.BranchTopping(BranchId,ToppingId,IsAvailable)
SELECT b.Id,t.Id,1 FROM @Branches b CROSS JOIN @Toppings x JOIN dbo.Topping t ON t.Name=x.TargetName AND t.Deleted=0
WHERE NOT EXISTS(SELECT 1 FROM dbo.BranchTopping bt WHERE bt.BranchId=b.Id AND bt.ToppingId=t.Id);

IF (SELECT COUNT(*) FROM @Prices)<>186
    THROW 51020,N'Không khớp 186 mức giá.',1;
SELECT CASE WHEN @Apply=1 THEN N'Đã nạp' ELSE N'Kiểm tra, sẽ rollback' END AS Result,
 (SELECT COUNT(*) FROM @Menu) AS Products,(SELECT COUNT(*) FROM @Prices) AS ProductSizes,
 (SELECT COUNT(*) FROM @Toppings) AS Toppings,(SELECT COUNT(*) FROM @Branches) AS Branches;
SELECT ProductName AS SourceName,TargetName FROM @Menu WHERE ProductName<>TargetName;
IF @Apply=1 COMMIT TRANSACTION; ELSE ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
THROW;
END CATCH;
