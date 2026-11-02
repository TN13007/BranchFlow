-- BranchFlow nhóm: baseline v1, SQL Server 2019+.
-- Chạy bằng sqlcmd -d BranchFlow_Dev (hoặc BranchFlow_Test), -b để dừng khi lỗi.
-- Chỉ áp dụng DB local mới; không xóa bảng, không reset dữ liệu.
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
IF DB_NAME() NOT IN (N'BranchFlow_Dev', N'BranchFlow_Test')
    THROW 51000, N'Chỉ cho phép BranchFlow_Dev hoặc BranchFlow_Test.', 1;
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 51000, N'DB đã có bảng; baseline không được chạy đè.', 1;
BEGIN TRANSACTION;


CREATE TABLE dbo.[Role]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Code] NVARCHAR(30) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Role_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Role_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Role_CodeNotBlank] CHECK (LEN(LTRIM(RTRIM([Code])))>0),
    CONSTRAINT [CK_Role_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[AppUser]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [RoleId] INT NOT NULL,
    [Username] NVARCHAR(100) NOT NULL,
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [FullName] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(254) NULL,
    [Phone] NVARCHAR(30) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_AppUser_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_AppUser_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_AppUser_Deleted] DEFAULT (0),
    CONSTRAINT [PK_AppUser] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AppUser_UsernameNotBlank] CHECK (LEN(LTRIM(RTRIM([Username])))>0),
    CONSTRAINT [CK_AppUser_PasswordHashNotBlank] CHECK (LEN(LTRIM(RTRIM([PasswordHash])))>0),
    CONSTRAINT [CK_AppUser_FullNameNotBlank] CHECK (LEN(LTRIM(RTRIM([FullName])))>0)
);


CREATE TABLE dbo.[Branch]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Code] NVARCHAR(30) NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Branch_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Branch_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Branch_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Branch] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Branch_CodeNotBlank] CHECK (LEN(LTRIM(RTRIM([Code])))>0),
    CONSTRAINT [CK_Branch_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[EmployeeProfile]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [EmployeeCode] NVARCHAR(30) NOT NULL,
    [DateOfBirth] DATE NULL,
    [HireDate] DATE NOT NULL,
    [LeaveDate] DATE NULL,
    [Position] NVARCHAR(100) NULL,
    [Address] NVARCHAR(500) NULL,
    [AvatarUrl] NVARCHAR(1000) NULL,
    [BaseSalary] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_EmployeeProfile_BaseSalary] DEFAULT (0),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_EmployeeProfile_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_EmployeeProfile_Deleted] DEFAULT (0),
    CONSTRAINT [PK_EmployeeProfile] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_EmployeeProfile_HireLeaveDate] CHECK (LeaveDate IS NULL OR LeaveDate >= HireDate),
    CONSTRAINT [CK_EmployeeProfile_BaseSalary] CHECK ([BaseSalary]>=0)
);


CREATE TABLE dbo.[AdminBranchAssignment]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [AdminUserId] INT NOT NULL,
    [BranchId] INT NOT NULL,
    [EffectiveFrom] DATETIME2(7) NOT NULL,
    [EffectiveTo] DATETIME2(7) NULL,
    [AssignedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_AdminBranchAssignment_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_AdminBranchAssignment] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AdminBranchAssignment_EffectiveRange] CHECK (EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom)
);


CREATE TABLE dbo.[EmployeeBranchAssignment]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EmployeeProfileId] INT NOT NULL,
    [BranchId] INT NOT NULL,
    [EffectiveFrom] DATETIME2(7) NOT NULL,
    [EffectiveTo] DATETIME2(7) NULL,
    [AssignedByUserId] INT NOT NULL,
    [TransferRequestId] INT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_EmployeeBranchAssignment_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_EmployeeBranchAssignment] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_EmployeeBranchAssignment_EffectiveRange] CHECK (EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom)
);


CREATE TABLE dbo.[EmployeeTransferRequest]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EmployeeProfileId] INT NOT NULL,
    [FromBranchId] INT NOT NULL,
    [ToBranchId] INT NOT NULL,
    [RequestedByUserId] INT NOT NULL,
    [ReviewedByUserId] INT NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_EmployeeTransferRequest_Status] DEFAULT (N'PENDING'),
    [Reason] NVARCHAR(500) NULL,
    [RejectionReason] NVARCHAR(500) NULL,
    [RequestedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_EmployeeTransferRequest_RequestedAt] DEFAULT (SYSUTCDATETIME()),
    [ReviewedAt] DATETIME2(7) NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_EmployeeTransferRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_EmployeeTransferRequest_Branches] CHECK (FromBranchId <> ToBranchId),
    CONSTRAINT [CK_EmployeeTransferRequest_Status] CHECK (Status IN (N'PENDING',N'APPROVED',N'REJECTED')),
    CONSTRAINT [CK_EmployeeTransferRequest_Review] CHECK ((Status=N'PENDING' AND ReviewedAt IS NULL AND ReviewedByUserId IS NULL) OR (Status IN (N'APPROVED',N'REJECTED') AND ReviewedAt IS NOT NULL AND ReviewedByUserId IS NOT NULL)),
    CONSTRAINT [CK_EmployeeTransferRequest_Rejection] CHECK (Status<>N'REJECTED' OR (RejectionReason IS NOT NULL AND LEN(LTRIM(RTRIM(RejectionReason)))>0))
);


CREATE TABLE dbo.[EmployeeStockPermission]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EmployeeProfileId] INT NOT NULL,
    [BranchId] INT NOT NULL,
    [CanReceive] BIT NOT NULL CONSTRAINT [DF_EmployeeStockPermission_CanReceive] DEFAULT (0),
    [CanIssue] BIT NOT NULL CONSTRAINT [DF_EmployeeStockPermission_CanIssue] DEFAULT (0),
    [CanStocktake] BIT NOT NULL CONSTRAINT [DF_EmployeeStockPermission_CanStocktake] DEFAULT (0),
    CONSTRAINT [PK_EmployeeStockPermission] PRIMARY KEY ([Id])
);


CREATE TABLE dbo.[Category]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Category_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Category_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Category_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Category] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Category_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[Product]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [CategoryId] INT NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [ImageUrl] NVARCHAR(1000) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Product_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Product_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Product_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Product] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Product_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[Size]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Size_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Size_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Size_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Size] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Size_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[ProductSize]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [SizeId] INT NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_ProductSize_Price] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ProductSize_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_ProductSize_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_ProductSize_Deleted] DEFAULT (0),
    CONSTRAINT [PK_ProductSize] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ProductSize_Price] CHECK ([Price]>=0)
);


CREATE TABLE dbo.[ToppingGroup]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_ToppingGroup_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_ToppingGroup_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_ToppingGroup_Deleted] DEFAULT (0),
    CONSTRAINT [PK_ToppingGroup] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ToppingGroup_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[Topping]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [ToppingGroupId] INT NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Topping_Price] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Topping_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Topping_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Topping_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Topping] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Topping_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0),
    CONSTRAINT [CK_Topping_Price] CHECK ([Price]>=0)
);


CREATE TABLE dbo.[NoteGroup]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_NoteGroup_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_NoteGroup_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_NoteGroup_Deleted] DEFAULT (0),
    CONSTRAINT [PK_NoteGroup] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NoteGroup_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[NoteOption]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [NoteGroupId] INT NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_NoteOption_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_NoteOption_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_NoteOption_Deleted] DEFAULT (0),
    CONSTRAINT [PK_NoteOption] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NoteOption_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[BranchProduct]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BranchId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [IsAvailable] BIT NOT NULL CONSTRAINT [DF_BranchProduct_IsAvailable] DEFAULT (1),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_BranchProduct] PRIMARY KEY ([Id])
);


CREATE TABLE dbo.[BranchTopping]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BranchId] INT NOT NULL,
    [ToppingId] INT NOT NULL,
    [IsAvailable] BIT NOT NULL CONSTRAINT [DF_BranchTopping_IsAvailable] DEFAULT (1),
    [UpdatedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_BranchTopping] PRIMARY KEY ([Id])
);


CREATE TABLE dbo.[SalesOrder]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Code] NVARCHAR(50) NOT NULL,
    [BranchId] INT NOT NULL,
    [CreatedByUserId] INT NOT NULL,
    [BusinessDate] DATE NOT NULL,
    [DailySequence] INT NOT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_SalesOrder_TotalAmount] DEFAULT (0),
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_SalesOrder_Status] DEFAULT (N'COMPLETED'),
    [ReportReason] NVARCHAR(500) NULL,
    [ReportedByUserId] INT NULL,
    [ReportedAt] DATETIME2(7) NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_SalesOrder_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_SalesOrder] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SalesOrder_DailySequence] CHECK (DailySequence>0),
    CONSTRAINT [CK_SalesOrder_Status] CHECK (Status IN (N'COMPLETED',N'NEEDS_REVIEW',N'CANCELLED')),
    CONSTRAINT [CK_SalesOrder_Review] CHECK (Status<>N'NEEDS_REVIEW' OR (ReportReason IS NOT NULL AND LEN(LTRIM(RTRIM(ReportReason)))>0 AND ReportedByUserId IS NOT NULL AND ReportedAt IS NOT NULL)),
    CONSTRAINT [CK_SalesOrder_CodeNotBlank] CHECK (LEN(LTRIM(RTRIM([Code])))>0),
    CONSTRAINT [CK_SalesOrder_TotalAmount] CHECK ([TotalAmount]>=0)
);


CREATE TABLE dbo.[OrderDailyCounter]
(
    [BranchId] INT NOT NULL,
    [BusinessDate] DATE NOT NULL,
    [LastNumber] INT NOT NULL,
    [UpdatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_OrderDailyCounter_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_OrderDailyCounter] PRIMARY KEY ([BranchId], [BusinessDate]),
    CONSTRAINT [CK_OrderDailyCounter_LastNumber] CHECK (LastNumber>=0)
);


CREATE TABLE dbo.[OrderItem]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SalesOrderId] INT NOT NULL,
    [ProductSizeId] INT NOT NULL,
    [ProductNameSnapshot] NVARCHAR(150) NOT NULL,
    [SizeNameSnapshot] NVARCHAR(50) NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPriceSnapshot] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_OrderItem_UnitPriceSnapshot] DEFAULT (0),
    [SubtotalAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_OrderItem_SubtotalAmount] DEFAULT (0),
    [Note] NVARCHAR(500) NULL,
    CONSTRAINT [PK_OrderItem] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderItem_Quantity] CHECK (Quantity>0),
    CONSTRAINT [CK_OrderItem_ProductNameSnapshotNotBlank] CHECK (LEN(LTRIM(RTRIM([ProductNameSnapshot])))>0),
    CONSTRAINT [CK_OrderItem_SizeNameSnapshotNotBlank] CHECK (LEN(LTRIM(RTRIM([SizeNameSnapshot])))>0),
    CONSTRAINT [CK_OrderItem_UnitPriceSnapshot] CHECK ([UnitPriceSnapshot]>=0),
    CONSTRAINT [CK_OrderItem_SubtotalAmount] CHECK ([SubtotalAmount]>=0)
);


CREATE TABLE dbo.[OrderItemTopping]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [OrderItemId] INT NOT NULL,
    [ToppingId] INT NOT NULL,
    [ToppingNameSnapshot] NVARCHAR(150) NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPriceSnapshot] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_OrderItemTopping_UnitPriceSnapshot] DEFAULT (0),
    CONSTRAINT [PK_OrderItemTopping] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderItemTopping_Quantity] CHECK (Quantity BETWEEN 1 AND 3),
    CONSTRAINT [CK_OrderItemTopping_ToppingNameSnapshotNotBlank] CHECK (LEN(LTRIM(RTRIM([ToppingNameSnapshot])))>0),
    CONSTRAINT [CK_OrderItemTopping_UnitPriceSnapshot] CHECK ([UnitPriceSnapshot]>=0)
);


CREATE TABLE dbo.[OrderItemNote]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [OrderItemId] INT NOT NULL,
    [NoteOptionId] INT NOT NULL,
    [NoteNameSnapshot] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_OrderItemNote] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderItemNote_NoteNameSnapshotNotBlank] CHECK (LEN(LTRIM(RTRIM([NoteNameSnapshot])))>0)
);


CREATE TABLE dbo.[OrderAdjustment]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [SalesOrderId] INT NOT NULL,
    [AdjustedByUserId] INT NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [BeforeData] NVARCHAR(MAX) NOT NULL,
    [AfterData] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_OrderAdjustment_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_OrderAdjustment] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_OrderAdjustment_ReasonNotBlank] CHECK (LEN(LTRIM(RTRIM([Reason])))>0),
    CONSTRAINT [CK_OrderAdjustment_BeforeDataJson] CHECK (ISJSON([BeforeData])=1),
    CONSTRAINT [CK_OrderAdjustment_AfterDataJson] CHECK (ISJSON([AfterData])=1)
);


CREATE TABLE dbo.[IngredientUnit]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_IngredientUnit_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_IngredientUnit_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_IngredientUnit_Deleted] DEFAULT (0),
    CONSTRAINT [PK_IngredientUnit] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_IngredientUnit_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[Ingredient]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UnitId] INT NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Ingredient_IsActive] DEFAULT (1),
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Ingredient_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NULL,
    [Deleted] BIT NOT NULL CONSTRAINT [DF_Ingredient_Deleted] DEFAULT (0),
    CONSTRAINT [PK_Ingredient] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Ingredient_NameNotBlank] CHECK (LEN(LTRIM(RTRIM([Name])))>0)
);


CREATE TABLE dbo.[BranchIngredient]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BranchId] INT NOT NULL,
    [IngredientId] INT NOT NULL,
    [Quantity] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_BranchIngredient_Quantity] DEFAULT (0),
    [WarningThreshold] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_BranchIngredient_WarningThreshold] DEFAULT (0),
    [UpdatedAt] DATETIME2(7) NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_BranchIngredient] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BranchIngredient_Quantity] CHECK ([Quantity]>=0),
    CONSTRAINT [CK_BranchIngredient_WarningThreshold] CHECK ([WarningThreshold]>=0)
);


CREATE TABLE dbo.[StockTransaction]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BranchId] INT NOT NULL,
    [PerformedByUserId] INT NOT NULL,
    [Type] NVARCHAR(30) NOT NULL,
    [OriginalTransactionId] INT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_StockTransaction_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_StockTransaction] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockTransaction_Type] CHECK (Type IN (N'IN',N'OUT',N'REVERSAL',N'COUNT_ADJUSTMENT')),
    CONSTRAINT [CK_StockTransaction_Original] CHECK ((Type=N'REVERSAL' AND OriginalTransactionId IS NOT NULL AND OriginalTransactionId<>Id) OR (Type<>N'REVERSAL' AND OriginalTransactionId IS NULL))
);


CREATE TABLE dbo.[StockTransactionDetail]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [StockTransactionId] INT NOT NULL,
    [IngredientId] INT NOT NULL,
    [QuantityChange] DECIMAL(18,3) NOT NULL,
    [QuantityBefore] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_StockTransactionDetail_QuantityBefore] DEFAULT (0),
    [QuantityAfter] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_StockTransactionDetail_QuantityAfter] DEFAULT (0),
    CONSTRAINT [PK_StockTransactionDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockTransactionDetail_Change] CHECK (QuantityChange<>0),
    CONSTRAINT [CK_StockTransactionDetail_Balance] CHECK (QuantityAfter=QuantityBefore+QuantityChange),
    CONSTRAINT [CK_StockTransactionDetail_QuantityBefore] CHECK ([QuantityBefore]>=0),
    CONSTRAINT [CK_StockTransactionDetail_QuantityAfter] CHECK ([QuantityAfter]>=0)
);


CREATE TABLE dbo.[Stocktake]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Code] NVARCHAR(50) NOT NULL,
    [BranchId] INT NOT NULL,
    [CheckedByUserId] INT NOT NULL,
    [AdjustmentTransactionId] INT NULL,
    [CompletedAt] DATETIME2(7) NULL,
    [Status] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Stocktake_Status] DEFAULT (N'DRAFT'),
    [ReviewedByUserId] INT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Stocktake_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [ReviewedAt] DATETIME2(7) NULL,
    [ReviewReason] NVARCHAR(500) NULL,
    [SubmittedAt] DATETIME2(7) NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_Stocktake] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Stocktake_Status] CHECK (Status IN (N'DRAFT',N'SUBMITTED',N'APPROVED',N'REJECTED')),
    CONSTRAINT [CK_Stocktake_Submitted] CHECK (Status=N'DRAFT' OR SubmittedAt IS NOT NULL),
    CONSTRAINT [CK_Stocktake_Review] CHECK ((Status IN (N'DRAFT',N'SUBMITTED') AND ReviewedByUserId IS NULL AND ReviewedAt IS NULL AND CompletedAt IS NULL) OR (Status=N'REJECTED' AND ReviewedByUserId IS NOT NULL AND ReviewedAt IS NOT NULL AND CompletedAt IS NULL AND ReviewReason IS NOT NULL AND LEN(LTRIM(RTRIM(ReviewReason)))>0) OR (Status=N'APPROVED' AND ReviewedByUserId IS NOT NULL AND ReviewedAt IS NOT NULL AND CompletedAt IS NOT NULL)),
    CONSTRAINT [CK_Stocktake_CodeNotBlank] CHECK (LEN(LTRIM(RTRIM([Code])))>0)
);


CREATE TABLE dbo.[StocktakeItem]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [StocktakeId] INT NOT NULL,
    [BranchIngredientId] INT NOT NULL,
    [SystemQuantity] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_StocktakeItem_SystemQuantity] DEFAULT (0),
    [ActualQuantity] DECIMAL(18,3) NOT NULL CONSTRAINT [DF_StocktakeItem_ActualQuantity] DEFAULT (0),
    [InventoryRowVersionSnapshot] BINARY(8) NOT NULL,
    [Difference] AS CONVERT(DECIMAL(18,3), [ActualQuantity]-[SystemQuantity]) PERSISTED NOT NULL,
    CONSTRAINT [PK_StocktakeItem] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StocktakeItem_SystemQuantity] CHECK ([SystemQuantity]>=0),
    CONSTRAINT [CK_StocktakeItem_ActualQuantity] CHECK ([ActualQuantity]>=0)
);


CREATE TABLE dbo.[Payroll]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [EmployeeProfileId] INT NOT NULL,
    [BranchId] INT NOT NULL,
    [Year] SMALLINT NOT NULL,
    [Month] TINYINT NOT NULL,
    [BaseSalarySnapshot] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Payroll_BaseSalarySnapshot] DEFAULT (0),
    [TotalSalary] AS CONVERT(DECIMAL(18,2), [BaseSalarySnapshot]+[AllowanceAmount]+[BonusAmount]-[DeductionAmount]) PERSISTED NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Payroll_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [UpdatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_Payroll_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
    [RowVersion] ROWVERSION NOT NULL,
    [AllowanceAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Payroll_AllowanceAmount] DEFAULT (0),
    [BonusAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Payroll_BonusAmount] DEFAULT (0),
    [DeductionAmount] DECIMAL(18,2) NOT NULL CONSTRAINT [DF_Payroll_DeductionAmount] DEFAULT (0),
    [CreatedByUserId] INT NOT NULL,
    [UpdatedByUserId] INT NOT NULL,
    CONSTRAINT [PK_Payroll] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Payroll_Year] CHECK ([Year] BETWEEN 2020 AND 2100),
    CONSTRAINT [CK_Payroll_Month] CHECK ([Month] BETWEEN 1 AND 12),
    CONSTRAINT [CK_Payroll_BaseSalarySnapshot] CHECK ([BaseSalarySnapshot]>=0),
    CONSTRAINT [CK_Payroll_TotalSalary] CHECK ([TotalSalary]>=0),
    CONSTRAINT [CK_Payroll_AllowanceAmount] CHECK ([AllowanceAmount]>=0),
    CONSTRAINT [CK_Payroll_BonusAmount] CHECK ([BonusAmount]>=0),
    CONSTRAINT [CK_Payroll_DeductionAmount] CHECK ([DeductionAmount]>=0)
);


CREATE TABLE dbo.[AuditLog]
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [BranchId] INT NULL,
    [EntityName] NVARCHAR(100) NOT NULL,
    [EntityId] INT NOT NULL,
    [Action] NVARCHAR(20) NOT NULL,
    [BeforeData] NVARCHAR(MAX) NULL,
    [AfterData] NVARCHAR(MAX) NULL,
    [PerformedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_AuditLog_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuditLog_Action] CHECK ([Action] IN (N'CREATE',N'UPDATE',N'SOFT_DELETE',N'RESTORE',N'ADJUST',N'REVERSE')),
    CONSTRAINT [CK_AuditLog_EntityNameNotBlank] CHECK (LEN(LTRIM(RTRIM([EntityName])))>0),
    CONSTRAINT [CK_AuditLog_BeforeDataJson] CHECK ([BeforeData] IS NULL OR ISJSON([BeforeData])=1),
    CONSTRAINT [CK_AuditLog_AfterDataJson] CHECK ([AfterData] IS NULL OR ISJSON([AfterData])=1)
);


CREATE TABLE dbo.[AuthSession]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [UserId] INT NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_AuthSession_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [ExpiresAt] DATETIME2(7) NOT NULL,
    [RevokedAt] DATETIME2(7) NULL,
    CONSTRAINT [PK_AuthSession] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AuthSession_Expiry] CHECK (ExpiresAt>CreatedAt),
    CONSTRAINT [CK_AuthSession_Revoked] CHECK (RevokedAt IS NULL OR RevokedAt>=CreatedAt)
);


CREATE TABLE dbo.[RefreshToken]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [TokenHash] VARCHAR(64) NOT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT [DF_RefreshToken_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [ExpiresAt] DATETIME2(7) NOT NULL,
    [UsedAt] DATETIME2(7) NULL,
    [RevokedAt] DATETIME2(7) NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [PK_RefreshToken] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RefreshToken_Expiry] CHECK (ExpiresAt>CreatedAt),
    CONSTRAINT [CK_RefreshToken_HashLength] CHECK (DATALENGTH(TokenHash)=64),
    CONSTRAINT [CK_RefreshToken_Used] CHECK (UsedAt IS NULL OR UsedAt>=CreatedAt),
    CONSTRAINT [CK_RefreshToken_Revoked] CHECK (RevokedAt IS NULL OR RevokedAt>=CreatedAt)
);


ALTER TABLE dbo.[AppUser] ADD CONSTRAINT [FK_AppUser_RoleId] FOREIGN KEY ([RoleId]) REFERENCES dbo.[Role] ([Id]);

ALTER TABLE dbo.[EmployeeProfile] ADD CONSTRAINT [FK_EmployeeProfile_UserId] FOREIGN KEY ([UserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[AdminBranchAssignment] ADD CONSTRAINT [FK_AdminBranchAssignment_AdminUserId] FOREIGN KEY ([AdminUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[AdminBranchAssignment] ADD CONSTRAINT [FK_AdminBranchAssignment_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[AdminBranchAssignment] ADD CONSTRAINT [FK_AdminBranchAssignment_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[EmployeeBranchAssignment] ADD CONSTRAINT [FK_EmployeeBranchAssignment_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES dbo.[EmployeeProfile] ([Id]);

ALTER TABLE dbo.[EmployeeBranchAssignment] ADD CONSTRAINT [FK_EmployeeBranchAssignment_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[EmployeeBranchAssignment] ADD CONSTRAINT [FK_EmployeeBranchAssignment_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[EmployeeBranchAssignment] ADD CONSTRAINT [FK_EmployeeBranchAssignment_TransferRequestId] FOREIGN KEY ([TransferRequestId]) REFERENCES dbo.[EmployeeTransferRequest] ([Id]);

ALTER TABLE dbo.[EmployeeTransferRequest] ADD CONSTRAINT [FK_EmployeeTransferRequest_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES dbo.[EmployeeProfile] ([Id]);

ALTER TABLE dbo.[EmployeeTransferRequest] ADD CONSTRAINT [FK_EmployeeTransferRequest_FromBranchId] FOREIGN KEY ([FromBranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[EmployeeTransferRequest] ADD CONSTRAINT [FK_EmployeeTransferRequest_ToBranchId] FOREIGN KEY ([ToBranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[EmployeeTransferRequest] ADD CONSTRAINT [FK_EmployeeTransferRequest_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[EmployeeTransferRequest] ADD CONSTRAINT [FK_EmployeeTransferRequest_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[EmployeeStockPermission] ADD CONSTRAINT [FK_EmployeeStockPermission_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES dbo.[EmployeeProfile] ([Id]);

ALTER TABLE dbo.[EmployeeStockPermission] ADD CONSTRAINT [FK_EmployeeStockPermission_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[Product] ADD CONSTRAINT [FK_Product_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES dbo.[Category] ([Id]);

ALTER TABLE dbo.[ProductSize] ADD CONSTRAINT [FK_ProductSize_ProductId] FOREIGN KEY ([ProductId]) REFERENCES dbo.[Product] ([Id]);

ALTER TABLE dbo.[ProductSize] ADD CONSTRAINT [FK_ProductSize_SizeId] FOREIGN KEY ([SizeId]) REFERENCES dbo.[Size] ([Id]);

ALTER TABLE dbo.[Topping] ADD CONSTRAINT [FK_Topping_ToppingGroupId] FOREIGN KEY ([ToppingGroupId]) REFERENCES dbo.[ToppingGroup] ([Id]);

ALTER TABLE dbo.[NoteOption] ADD CONSTRAINT [FK_NoteOption_NoteGroupId] FOREIGN KEY ([NoteGroupId]) REFERENCES dbo.[NoteGroup] ([Id]);

ALTER TABLE dbo.[BranchProduct] ADD CONSTRAINT [FK_BranchProduct_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[BranchProduct] ADD CONSTRAINT [FK_BranchProduct_ProductId] FOREIGN KEY ([ProductId]) REFERENCES dbo.[Product] ([Id]);

ALTER TABLE dbo.[BranchTopping] ADD CONSTRAINT [FK_BranchTopping_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[BranchTopping] ADD CONSTRAINT [FK_BranchTopping_ToppingId] FOREIGN KEY ([ToppingId]) REFERENCES dbo.[Topping] ([Id]);

ALTER TABLE dbo.[SalesOrder] ADD CONSTRAINT [FK_SalesOrder_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[SalesOrder] ADD CONSTRAINT [FK_SalesOrder_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[SalesOrder] ADD CONSTRAINT [FK_SalesOrder_ReportedByUserId] FOREIGN KEY ([ReportedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[OrderDailyCounter] ADD CONSTRAINT [FK_OrderDailyCounter_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[OrderItem] ADD CONSTRAINT [FK_OrderItem_SalesOrderId] FOREIGN KEY ([SalesOrderId]) REFERENCES dbo.[SalesOrder] ([Id]);

ALTER TABLE dbo.[OrderItem] ADD CONSTRAINT [FK_OrderItem_ProductSizeId] FOREIGN KEY ([ProductSizeId]) REFERENCES dbo.[ProductSize] ([Id]);

ALTER TABLE dbo.[OrderItemTopping] ADD CONSTRAINT [FK_OrderItemTopping_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES dbo.[OrderItem] ([Id]);

ALTER TABLE dbo.[OrderItemTopping] ADD CONSTRAINT [FK_OrderItemTopping_ToppingId] FOREIGN KEY ([ToppingId]) REFERENCES dbo.[Topping] ([Id]);

ALTER TABLE dbo.[OrderItemNote] ADD CONSTRAINT [FK_OrderItemNote_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES dbo.[OrderItem] ([Id]);

ALTER TABLE dbo.[OrderItemNote] ADD CONSTRAINT [FK_OrderItemNote_NoteOptionId] FOREIGN KEY ([NoteOptionId]) REFERENCES dbo.[NoteOption] ([Id]);

ALTER TABLE dbo.[OrderAdjustment] ADD CONSTRAINT [FK_OrderAdjustment_SalesOrderId] FOREIGN KEY ([SalesOrderId]) REFERENCES dbo.[SalesOrder] ([Id]);

ALTER TABLE dbo.[OrderAdjustment] ADD CONSTRAINT [FK_OrderAdjustment_AdjustedByUserId] FOREIGN KEY ([AdjustedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[Ingredient] ADD CONSTRAINT [FK_Ingredient_UnitId] FOREIGN KEY ([UnitId]) REFERENCES dbo.[IngredientUnit] ([Id]);

ALTER TABLE dbo.[BranchIngredient] ADD CONSTRAINT [FK_BranchIngredient_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[BranchIngredient] ADD CONSTRAINT [FK_BranchIngredient_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES dbo.[Ingredient] ([Id]);

ALTER TABLE dbo.[StockTransaction] ADD CONSTRAINT [FK_StockTransaction_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[StockTransaction] ADD CONSTRAINT [FK_StockTransaction_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[StockTransaction] ADD CONSTRAINT [FK_StockTransaction_OriginalTransactionId] FOREIGN KEY ([OriginalTransactionId]) REFERENCES dbo.[StockTransaction] ([Id]);

ALTER TABLE dbo.[StockTransactionDetail] ADD CONSTRAINT [FK_StockTransactionDetail_StockTransactionId] FOREIGN KEY ([StockTransactionId]) REFERENCES dbo.[StockTransaction] ([Id]);

ALTER TABLE dbo.[StockTransactionDetail] ADD CONSTRAINT [FK_StockTransactionDetail_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES dbo.[Ingredient] ([Id]);

ALTER TABLE dbo.[Stocktake] ADD CONSTRAINT [FK_Stocktake_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[Stocktake] ADD CONSTRAINT [FK_Stocktake_CheckedByUserId] FOREIGN KEY ([CheckedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[Stocktake] ADD CONSTRAINT [FK_Stocktake_AdjustmentTransactionId] FOREIGN KEY ([AdjustmentTransactionId]) REFERENCES dbo.[StockTransaction] ([Id]);

ALTER TABLE dbo.[Stocktake] ADD CONSTRAINT [FK_Stocktake_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[StocktakeItem] ADD CONSTRAINT [FK_StocktakeItem_StocktakeId] FOREIGN KEY ([StocktakeId]) REFERENCES dbo.[Stocktake] ([Id]);

ALTER TABLE dbo.[StocktakeItem] ADD CONSTRAINT [FK_StocktakeItem_BranchIngredientId] FOREIGN KEY ([BranchIngredientId]) REFERENCES dbo.[BranchIngredient] ([Id]);

ALTER TABLE dbo.[Payroll] ADD CONSTRAINT [FK_Payroll_EmployeeProfileId] FOREIGN KEY ([EmployeeProfileId]) REFERENCES dbo.[EmployeeProfile] ([Id]);

ALTER TABLE dbo.[Payroll] ADD CONSTRAINT [FK_Payroll_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[Payroll] ADD CONSTRAINT [FK_Payroll_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[Payroll] ADD CONSTRAINT [FK_Payroll_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[AuditLog] ADD CONSTRAINT [FK_AuditLog_BranchId] FOREIGN KEY ([BranchId]) REFERENCES dbo.[Branch] ([Id]);

ALTER TABLE dbo.[AuditLog] ADD CONSTRAINT [FK_AuditLog_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[AuthSession] ADD CONSTRAINT [FK_AuthSession_UserId] FOREIGN KEY ([UserId]) REFERENCES dbo.[AppUser] ([Id]);

ALTER TABLE dbo.[RefreshToken] ADD CONSTRAINT [FK_RefreshToken_SessionId] FOREIGN KEY ([SessionId]) REFERENCES dbo.[AuthSession] ([Id]);

CREATE UNIQUE INDEX [UX_Role_Code] ON dbo.[Role] ([Code]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_Branch_Code] ON dbo.[Branch] ([Code]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_AppUser_Username] ON dbo.[AppUser] ([Username]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_AppUser_Email] ON dbo.[AppUser] ([Email]) WHERE Deleted=0 AND Email IS NOT NULL;

CREATE UNIQUE INDEX [UX_EmployeeProfile_UserId] ON dbo.[EmployeeProfile] ([UserId]);

CREATE UNIQUE INDEX [UX_EmployeeProfile_EmployeeCode] ON dbo.[EmployeeProfile] ([EmployeeCode]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_AdminBranchAssignment_OpenBranch] ON dbo.[AdminBranchAssignment] ([BranchId]) WHERE EffectiveTo IS NULL;

CREATE UNIQUE INDEX [UX_EmployeeBranchAssignment_OpenEmployee] ON dbo.[EmployeeBranchAssignment] ([EmployeeProfileId]) WHERE EffectiveTo IS NULL;

CREATE UNIQUE INDEX [UX_EmployeeStockPermission_EmployeeProfileId_BranchId] ON dbo.[EmployeeStockPermission] ([EmployeeProfileId], [BranchId]);

CREATE UNIQUE INDEX [UX_Category_Name] ON dbo.[Category] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_Product_Name] ON dbo.[Product] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_Size_Name] ON dbo.[Size] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_ToppingGroup_Name] ON dbo.[ToppingGroup] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_Topping_Name] ON dbo.[Topping] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_NoteGroup_Name] ON dbo.[NoteGroup] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_IngredientUnit_Name] ON dbo.[IngredientUnit] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_Ingredient_Name] ON dbo.[Ingredient] ([Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_NoteOption_NoteGroupId_Name] ON dbo.[NoteOption] ([NoteGroupId], [Name]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_ProductSize_ProductId_SizeId] ON dbo.[ProductSize] ([ProductId], [SizeId]) WHERE Deleted=0;

CREATE UNIQUE INDEX [UX_BranchProduct_BranchId_ProductId] ON dbo.[BranchProduct] ([BranchId], [ProductId]);

CREATE UNIQUE INDEX [UX_BranchTopping_BranchId_ToppingId] ON dbo.[BranchTopping] ([BranchId], [ToppingId]);

CREATE UNIQUE INDEX [UX_BranchIngredient_BranchId_IngredientId] ON dbo.[BranchIngredient] ([BranchId], [IngredientId]);

CREATE UNIQUE INDEX [UX_SalesOrder_Code] ON dbo.[SalesOrder] ([Code]);

CREATE UNIQUE INDEX [UX_SalesOrder_BranchId_BusinessDate_DailySequence] ON dbo.[SalesOrder] ([BranchId], [BusinessDate], [DailySequence]);

CREATE UNIQUE INDEX [UX_OrderItemTopping_OrderItemId_ToppingId] ON dbo.[OrderItemTopping] ([OrderItemId], [ToppingId]);

CREATE UNIQUE INDEX [UX_OrderItemNote_OrderItemId_NoteOptionId] ON dbo.[OrderItemNote] ([OrderItemId], [NoteOptionId]);

CREATE UNIQUE INDEX [UX_StockTransaction_OriginalTransactionId] ON dbo.[StockTransaction] ([OriginalTransactionId]) WHERE OriginalTransactionId IS NOT NULL;

CREATE UNIQUE INDEX [UX_StockTransactionDetail_StockTransactionId_IngredientId] ON dbo.[StockTransactionDetail] ([StockTransactionId], [IngredientId]);

CREATE UNIQUE INDEX [UX_Stocktake_Code] ON dbo.[Stocktake] ([Code]);

CREATE UNIQUE INDEX [UX_Stocktake_AdjustmentTransactionId] ON dbo.[Stocktake] ([AdjustmentTransactionId]) WHERE AdjustmentTransactionId IS NOT NULL;

CREATE UNIQUE INDEX [UX_StocktakeItem_StocktakeId_BranchIngredientId] ON dbo.[StocktakeItem] ([StocktakeId], [BranchIngredientId]);

CREATE UNIQUE INDEX [UX_Payroll_EmployeeProfileId_Year_Month] ON dbo.[Payroll] ([EmployeeProfileId], [Year], [Month]);

CREATE UNIQUE INDEX [UX_RefreshToken_TokenHash] ON dbo.[RefreshToken] ([TokenHash]);

CREATE INDEX [IX_AdminBranchAssignment_AdminUserId_EffectiveFrom_EffectiveTo] ON dbo.[AdminBranchAssignment] ([AdminUserId], [EffectiveFrom], [EffectiveTo]);

CREATE INDEX [IX_EmployeeBranchAssignment_BranchId_EffectiveFrom_EffectiveTo] ON dbo.[EmployeeBranchAssignment] ([BranchId], [EffectiveFrom], [EffectiveTo]);

CREATE INDEX [IX_EmployeeTransferRequest_EmployeeProfileId_Status] ON dbo.[EmployeeTransferRequest] ([EmployeeProfileId], [Status]);

CREATE INDEX [IX_SalesOrder_BranchId_BusinessDate_Status] ON dbo.[SalesOrder] ([BranchId], [BusinessDate], [Status]);

CREATE INDEX [IX_SalesOrder_CreatedByUserId_CreatedAt] ON dbo.[SalesOrder] ([CreatedByUserId], [CreatedAt]);

CREATE INDEX [IX_OrderAdjustment_SalesOrderId_CreatedAt] ON dbo.[OrderAdjustment] ([SalesOrderId], [CreatedAt]);

CREATE INDEX [IX_StockTransaction_BranchId_CreatedAt] ON dbo.[StockTransaction] ([BranchId], [CreatedAt]);

CREATE INDEX [IX_Stocktake_BranchId_CreatedAt] ON dbo.[Stocktake] ([BranchId], [CreatedAt]);

CREATE INDEX [IX_AuditLog_BranchId_CreatedAt] ON dbo.[AuditLog] ([BranchId], [CreatedAt]);

CREATE INDEX [IX_AuditLog_EntityName_EntityId_CreatedAt] ON dbo.[AuditLog] ([EntityName], [EntityId], [CreatedAt]);

CREATE INDEX [IX_AuthSession_UserId_ExpiresAt] ON dbo.[AuthSession] ([UserId], [ExpiresAt]);

CREATE INDEX [IX_RefreshToken_SessionId] ON dbo.[RefreshToken] ([SessionId]);

CREATE INDEX [IX_AppUser_RoleId_FK] ON dbo.[AppUser] ([RoleId]);

CREATE INDEX [IX_AdminBranchAssignment_BranchId_FK] ON dbo.[AdminBranchAssignment] ([BranchId]);

CREATE INDEX [IX_AdminBranchAssignment_AssignedByUserId_FK] ON dbo.[AdminBranchAssignment] ([AssignedByUserId]);

CREATE INDEX [IX_EmployeeBranchAssignment_EmployeeProfileId_FK] ON dbo.[EmployeeBranchAssignment] ([EmployeeProfileId]);

CREATE INDEX [IX_EmployeeBranchAssignment_AssignedByUserId_FK] ON dbo.[EmployeeBranchAssignment] ([AssignedByUserId]);

CREATE INDEX [IX_EmployeeBranchAssignment_TransferRequestId_FK] ON dbo.[EmployeeBranchAssignment] ([TransferRequestId]);

CREATE INDEX [IX_EmployeeTransferRequest_FromBranchId_FK] ON dbo.[EmployeeTransferRequest] ([FromBranchId]);

CREATE INDEX [IX_EmployeeTransferRequest_ToBranchId_FK] ON dbo.[EmployeeTransferRequest] ([ToBranchId]);

CREATE INDEX [IX_EmployeeTransferRequest_RequestedByUserId_FK] ON dbo.[EmployeeTransferRequest] ([RequestedByUserId]);

CREATE INDEX [IX_EmployeeTransferRequest_ReviewedByUserId_FK] ON dbo.[EmployeeTransferRequest] ([ReviewedByUserId]);

CREATE INDEX [IX_EmployeeStockPermission_BranchId_FK] ON dbo.[EmployeeStockPermission] ([BranchId]);

CREATE INDEX [IX_Product_CategoryId_FK] ON dbo.[Product] ([CategoryId]);

CREATE INDEX [IX_ProductSize_ProductId_FK] ON dbo.[ProductSize] ([ProductId]);

CREATE INDEX [IX_ProductSize_SizeId_FK] ON dbo.[ProductSize] ([SizeId]);

CREATE INDEX [IX_Topping_ToppingGroupId_FK] ON dbo.[Topping] ([ToppingGroupId]);

CREATE INDEX [IX_NoteOption_NoteGroupId_FK] ON dbo.[NoteOption] ([NoteGroupId]);

CREATE INDEX [IX_BranchProduct_ProductId_FK] ON dbo.[BranchProduct] ([ProductId]);

CREATE INDEX [IX_BranchTopping_ToppingId_FK] ON dbo.[BranchTopping] ([ToppingId]);

CREATE INDEX [IX_SalesOrder_ReportedByUserId_FK] ON dbo.[SalesOrder] ([ReportedByUserId]);

CREATE INDEX [IX_OrderItem_SalesOrderId_FK] ON dbo.[OrderItem] ([SalesOrderId]);

CREATE INDEX [IX_OrderItem_ProductSizeId_FK] ON dbo.[OrderItem] ([ProductSizeId]);

CREATE INDEX [IX_OrderItemTopping_ToppingId_FK] ON dbo.[OrderItemTopping] ([ToppingId]);

CREATE INDEX [IX_OrderItemNote_NoteOptionId_FK] ON dbo.[OrderItemNote] ([NoteOptionId]);

CREATE INDEX [IX_OrderAdjustment_AdjustedByUserId_FK] ON dbo.[OrderAdjustment] ([AdjustedByUserId]);

CREATE INDEX [IX_Ingredient_UnitId_FK] ON dbo.[Ingredient] ([UnitId]);

CREATE INDEX [IX_BranchIngredient_IngredientId_FK] ON dbo.[BranchIngredient] ([IngredientId]);

CREATE INDEX [IX_StockTransaction_PerformedByUserId_FK] ON dbo.[StockTransaction] ([PerformedByUserId]);

CREATE INDEX [IX_StockTransaction_OriginalTransactionId_FK] ON dbo.[StockTransaction] ([OriginalTransactionId]);

CREATE INDEX [IX_StockTransactionDetail_IngredientId_FK] ON dbo.[StockTransactionDetail] ([IngredientId]);

CREATE INDEX [IX_Stocktake_CheckedByUserId_FK] ON dbo.[Stocktake] ([CheckedByUserId]);

CREATE INDEX [IX_Stocktake_AdjustmentTransactionId_FK] ON dbo.[Stocktake] ([AdjustmentTransactionId]);

CREATE INDEX [IX_Stocktake_ReviewedByUserId_FK] ON dbo.[Stocktake] ([ReviewedByUserId]);

CREATE INDEX [IX_StocktakeItem_BranchIngredientId_FK] ON dbo.[StocktakeItem] ([BranchIngredientId]);

CREATE INDEX [IX_Payroll_BranchId_FK] ON dbo.[Payroll] ([BranchId]);

CREATE INDEX [IX_Payroll_CreatedByUserId_FK] ON dbo.[Payroll] ([CreatedByUserId]);

CREATE INDEX [IX_Payroll_UpdatedByUserId_FK] ON dbo.[Payroll] ([UpdatedByUserId]);

CREATE INDEX [IX_AuditLog_PerformedByUserId_FK] ON dbo.[AuditLog] ([PerformedByUserId]);

EXEC(N'CREATE PROCEDURE dbo.usp_GetNextOrderCode
    @BranchId INT,
    @BusinessDate DATE,
    @OrderCode NVARCHAR(50) OUTPUT,
    @SequenceNumber INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    -- Output của lần gọi trước không được ảnh hưởng lần cấp số tiếp theo.
    SET @OrderCode=NULL;
    SET @SequenceNumber=NULL;
    IF @BranchId IS NULL OR @BranchId<=0 OR @BusinessDate IS NULL
        THROW 51001, N''Chi nhánh/ngày nghiệp vụ không hợp lệ.'', 1;
    DECLARE @StartedTransaction BIT=0;
    DECLARE @BranchCode NVARCHAR(30);
    IF @@TRANCOUNT=0
    BEGIN
        BEGIN TRANSACTION;
        SET @StartedTransaction=1;
    END;
    BEGIN TRY
        -- Khóa branch và counter đến cuối transaction, đồng bộ cấp mã cùng branch.
        SELECT @BranchCode=Code FROM dbo.Branch WITH (UPDLOCK,HOLDLOCK)
        WHERE Id=@BranchId AND IsActive=1 AND Deleted=0;
        IF @BranchCode IS NULL
            THROW 51001, N''Chi nhánh không tồn tại hoặc không hoạt động.'',1;
        SELECT @SequenceNumber=LastNumber FROM dbo.OrderDailyCounter WITH (UPDLOCK,HOLDLOCK)
        WHERE BranchId=@BranchId AND BusinessDate=@BusinessDate;
        IF @SequenceNumber IS NULL
        BEGIN
            SET @SequenceNumber=1;
            INSERT dbo.OrderDailyCounter(BranchId,BusinessDate,LastNumber)
            VALUES(@BranchId,@BusinessDate,@SequenceNumber);
        END
        ELSE
        BEGIN
            IF @SequenceNumber=2147483647
                THROW 51002,N''Bộ đếm đã đạt giới hạn INT.'',1;
            SET @SequenceNumber=@SequenceNumber+1;
            UPDATE dbo.OrderDailyCounter SET LastNumber=@SequenceNumber,UpdatedAt=SYSUTCDATETIME()
            WHERE BranchId=@BranchId AND BusinessDate=@BusinessDate;
        END;
        -- Đệm tối thiểu 4 chữ số, giữ nguyên 10000 trở lên thay vì cắt bằng RIGHT(...,4).
        DECLARE @Digits NVARCHAR(10)=CONVERT(NVARCHAR(10),@SequenceNumber);
        SET @OrderCode=CONCAT(UPPER(@BranchCode),N''-'',CONVERT(CHAR(8),@BusinessDate,112),N''-'',
            REPLICATE(N''0'',CASE WHEN LEN(@Digits)<4 THEN 4-LEN(@Digits) ELSE 0 END),@Digits);
        IF @StartedTransaction=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @StartedTransaction=1 AND XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;');

COMMIT TRANSACTION;
PRINT N'Baseline v1: 35 bảng đã tạo.';
