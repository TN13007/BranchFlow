-- Dữ liệu cấu hình; không chứa user/mật khẩu/tồn kho/đơn hàng.
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
IF DB_NAME() NOT IN (N'BranchFlow_Dev',N'BranchFlow_Test')
    THROW 51000,N'Chỉ cho phép DB local của nhóm.',1;
BEGIN TRANSACTION;
INSERT dbo.[Role](Code,Name)
SELECT v.Code,v.Name FROM (VALUES
 (N'OWNER',N'Chủ sở hữu'),(N'SYSTEM_ADMIN',N'Admin Tổng'),
 (N'BRANCH_ADMIN',N'Admin Chi nhánh'),(N'EMPLOYEE',N'Nhân viên')
) v(Code,Name) WHERE NOT EXISTS(SELECT 1 FROM dbo.[Role] r WHERE r.Code=v.Code AND r.Deleted=0);
INSERT dbo.Size(Name) SELECT v.Name FROM (VALUES(N'M'),(N'L'))v(Name)
WHERE NOT EXISTS(SELECT 1 FROM dbo.Size s WHERE s.Name=v.Name AND s.Deleted=0);
INSERT dbo.IngredientUnit(Name) SELECT v.Name FROM (VALUES(N'kg'),(N'g'),(N'lít'),(N'ml'))v(Name)
WHERE NOT EXISTS(SELECT 1 FROM dbo.IngredientUnit u WHERE u.Name=v.Name AND u.Deleted=0);
COMMIT TRANSACTION;
