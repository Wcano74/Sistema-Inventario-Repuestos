
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Sales] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [UserId] nvarchar(max) NULL,
    CONSTRAINT [PK_Sales] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Suppliers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [ContactName] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Barcode] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [Cost] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [MinStock] int NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [CategoryId] int NOT NULL,
    [SupplierId] int NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id])
);
GO

CREATE TABLE [Adjustments] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [Reason] nvarchar(max) NULL,
    [UserId] nvarchar(max) NULL,
    CONSTRAINT [PK_Adjustments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Adjustments_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SaleDetails] (
    [Id] int NOT NULL IDENTITY,
    [SaleId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_SaleDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SaleDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SaleDetails_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Adjustments_ProductId] ON [Adjustments] ([ProductId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO

CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);
GO

CREATE INDEX [IX_SaleDetails_ProductId] ON [SaleDetails] ([ProductId]);
GO

CREATE INDEX [IX_SaleDetails_SaleId] ON [SaleDetails] ([SaleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227055937_InitialCreate', N'8.0.0');
GO



                CREATE PROCEDURE sp_GetDailySales
                AS
                BEGIN
                    SELECT 
                        ISNULL(SUM(Total), 0) as TotalSales,
                        COUNT(*) as TransactionCount
                    FROM Sales
                    WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE);
                END


                CREATE PROCEDURE sp_GetLowStockProducts
                AS
                BEGIN
                    SELECT * 
                    FROM Products
                    WHERE StockQuantity <= MinStock;
                END


                CREATE PROCEDURE sp_GetTopSellingProducts
                AS
                BEGIN
                    SELECT TOP 5
                        p.Name,
                        SUM(sd.Quantity) as TotalSold,
                        SUM(sd.Subtotal) as Revenue
                    FROM SaleDetails sd
                    JOIN Products p ON sd.ProductId = p.Id
                    GROUP BY p.Name
                    ORDER BY TotalSold DESC;
                END


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227062103_AddReportProcedures', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

                CREATE PROCEDURE sp_GetWeeklySales
                AS
                BEGIN
                    -- Get last 7 days including today
                    SELECT 
                        FORMAT(d.DateValue, 'dddd', 'es-ES') as DayName,
                        ISNULL(SUM(s.Total), 0) as TotalSales
                    FROM (
                        SELECT CAST(GETDATE() - 6 AS DATE) AS DateValue UNION ALL
                        SELECT CAST(GETDATE() - 5 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 4 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 3 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 2 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() - 1 AS DATE) UNION ALL
                        SELECT CAST(GETDATE() AS DATE)
                    ) d
                    LEFT JOIN Sales s ON CAST(s.Date AS DATE) = d.DateValue
                    GROUP BY d.DateValue
                    ORDER BY d.DateValue;
                END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227065727_AddWeeklySalesProcedure', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [SystemConfigurations] (
    [Key] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SystemConfigurations] PRIMARY KEY ([Key])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227205949_AddSystemConfigurations', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [Address] nvarchar(255) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [Age] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [AspNetUsers] ADD [DPI] nvarchar(20) NOT NULL DEFAULT N'';
GO

ALTER TABLE [AspNetUsers] ADD [EducationLevel] nvarchar(100) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [EmployeeCode] nvarchar(5) NOT NULL DEFAULT N'';
GO

ALTER TABLE [AspNetUsers] ADD [FirstName] nvarchar(100) NOT NULL DEFAULT N'';
GO

ALTER TABLE [AspNetUsers] ADD [LastName] nvarchar(100) NOT NULL DEFAULT N'';
GO

ALTER TABLE [AspNetUsers] ADD [ProfilePictureUrl] nvarchar(max) NULL;
GO

ALTER TABLE [AspNetUsers] ADD [Salary] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251228014618_UpdateApplicationUser', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Products] ADD [Brand] nvarchar(max) NULL;
GO

ALTER TABLE [Products] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [ProductHistories] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Action] nvarchar(max) NOT NULL,
    [QuantityChange] int NOT NULL,
    [NewStock] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [Date] datetime2 NOT NULL,
    CONSTRAINT [PK_ProductHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_ProductHistories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ProductHistories_ProductId] ON [ProductHistories] ([ProductId]);
GO

CREATE INDEX [IX_ProductHistories_UserId] ON [ProductHistories] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251229035531_AddProductHistoryAndBrand', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Sales] ADD [AmountPaid] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Sales] ADD [CashRegisterId] int NULL;
GO

ALTER TABLE [Sales] ADD [Change] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Sales] ADD [CustomerId] int NULL;
GO

ALTER TABLE [Sales] ADD [PaymentDetails] nvarchar(max) NULL;
GO

ALTER TABLE [Sales] ADD [PaymentMethod] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [CashRegisters] (
    [Id] int NOT NULL IDENTITY,
    [OpenedAt] datetime2 NOT NULL,
    [ClosedAt] datetime2 NULL,
    [OpeningBalance] decimal(18,2) NOT NULL,
    [ExpectedBalance] decimal(18,2) NOT NULL,
    [ActualBalance] decimal(18,2) NOT NULL,
    [Difference] decimal(18,2) NOT NULL,
    [OpeningNotes] nvarchar(max) NULL,
    [ClosingNotes] nvarchar(max) NULL,
    [IsOpen] bit NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_CashRegisters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CashRegisters_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [NitDpi] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [FiscalName] nvarchar(max) NULL,
    [FiscalAddress] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastPurchaseDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_Sales_CashRegisterId] ON [Sales] ([CashRegisterId]);
GO

CREATE INDEX [IX_Sales_CustomerId] ON [Sales] ([CustomerId]);
GO

CREATE INDEX [IX_CashRegisters_UserId] ON [CashRegisters] ([UserId]);
GO

ALTER TABLE [Sales] ADD CONSTRAINT [FK_Sales_CashRegisters_CashRegisterId] FOREIGN KEY ([CashRegisterId]) REFERENCES [CashRegisters] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [Sales] ADD CONSTRAINT [FK_Sales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230021752_AddCustomersAndCashRegister', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PurchaseOrders] (
    [Id] int NOT NULL IDENTITY,
    [SupplierId] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [ExpectedDate] datetime2 NULL,
    [Status] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseOrders_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [PurchaseOrderItems] (
    [Id] int NOT NULL IDENTITY,
    [PurchaseOrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitCost] decimal(18,2) NOT NULL,
    [TotalCost] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PurchaseOrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchaseOrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [PurchaseOrders] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_PurchaseOrderItems_ProductId] ON [PurchaseOrderItems] ([ProductId]);
GO

CREATE INDEX [IX_PurchaseOrderItems_PurchaseOrderId] ON [PurchaseOrderItems] ([PurchaseOrderId]);
GO

CREATE INDEX [IX_PurchaseOrders_SupplierId] ON [PurchaseOrders] ([SupplierId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231025434_AddPurchaseOrders', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PurchaseOrders] ADD [CanceledByUserId] nvarchar(450) NULL;
GO

ALTER TABLE [PurchaseOrders] ADD [CreatedByUserId] nvarchar(450) NULL;
GO

ALTER TABLE [PurchaseOrders] ADD [ReceivedByUserId] nvarchar(450) NULL;
GO

CREATE INDEX [IX_PurchaseOrders_CanceledByUserId] ON [PurchaseOrders] ([CanceledByUserId]);
GO

CREATE INDEX [IX_PurchaseOrders_CreatedByUserId] ON [PurchaseOrders] ([CreatedByUserId]);
GO

CREATE INDEX [IX_PurchaseOrders_ReceivedByUserId] ON [PurchaseOrders] ([ReceivedByUserId]);
GO

ALTER TABLE [PurchaseOrders] ADD CONSTRAINT [FK_PurchaseOrders_AspNetUsers_CanceledByUserId] FOREIGN KEY ([CanceledByUserId]) REFERENCES [AspNetUsers] ([Id]);
GO

ALTER TABLE [PurchaseOrders] ADD CONSTRAINT [FK_PurchaseOrders_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]);
GO

ALTER TABLE [PurchaseOrders] ADD CONSTRAINT [FK_PurchaseOrders_AspNetUsers_ReceivedByUserId] FOREIGN KEY ([ReceivedByUserId]) REFERENCES [AspNetUsers] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231033339_UpdatePurchaseOrderUsers', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [CanAccessPurchases] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [AspNetUsers] ADD [CanCreateOrders] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [AspNetUsers] ADD [CanManageOrders] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231034813_AddUserPermissions', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [CanViewAuditLog] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231050302_AddAuditLogPermission', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [InventoryCycles] (
    [Id] int NOT NULL IDENTITY,
    [OpenedAt] datetime2 NOT NULL,
    [ClosedAt] datetime2 NULL,
    [Status] int NOT NULL,
    [Scope] nvarchar(max) NOT NULL,
    [CategoryId] int NULL,
    [Notes] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_InventoryCycles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryCycles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryCycles_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id])
);
GO

CREATE TABLE [InventoryCounts] (
    [Id] int NOT NULL IDENTITY,
    [InventoryCycleId] int NOT NULL,
    [ProductId] int NOT NULL,
    [PhysicalQuantity] int NOT NULL,
    [SystemQuantityAtClose] int NOT NULL,
    [Difference] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [IsVerified] bit NOT NULL,
    CONSTRAINT [PK_InventoryCounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryCounts_InventoryCycles_InventoryCycleId] FOREIGN KEY ([InventoryCycleId]) REFERENCES [InventoryCycles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryCounts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_InventoryCounts_InventoryCycleId] ON [InventoryCounts] ([InventoryCycleId]);
GO

CREATE INDEX [IX_InventoryCounts_ProductId] ON [InventoryCounts] ([ProductId]);
GO

CREATE INDEX [IX_InventoryCycles_CategoryId] ON [InventoryCycles] ([CategoryId]);
GO

CREATE INDEX [IX_InventoryCycles_UserId] ON [InventoryCycles] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231204154_AddInventoryCycleEntities', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ExpenseCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Expenses] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Date] datetime2 NOT NULL,
    [Reference] nvarchar(max) NULL,
    [ExpenseCategoryId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Expenses_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Expenses_ExpenseCategories_ExpenseCategoryId] FOREIGN KEY ([ExpenseCategoryId]) REFERENCES [ExpenseCategories] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Expenses_ExpenseCategoryId] ON [Expenses] ([ExpenseCategoryId]);
GO

CREATE INDEX [IX_Expenses_UserId] ON [Expenses] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251231213007_AddExpenseEntities', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Sales] ADD [HasRefunds] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Sales] ADD [RefundedAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Sales] ADD [Status] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [SaleDetails] ADD [QuantityReturned] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [AspNetUsers] ADD [CanProcessRefunds] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [SaleRefunds] (
    [Id] int NOT NULL IDENTITY,
    [SaleId] int NOT NULL,
    [RefundDate] datetime2 NOT NULL,
    [UserId] nvarchar(450) NULL,
    [RefundType] int NOT NULL,
    [RefundAmount] decimal(18,2) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Notes] nvarchar(1000) NULL,
    [CashRegisterId] int NULL,
    [IsRegisteredAsExpense] bit NOT NULL,
    CONSTRAINT [PK_SaleRefunds] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SaleRefunds_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_SaleRefunds_CashRegisters_CashRegisterId] FOREIGN KEY ([CashRegisterId]) REFERENCES [CashRegisters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_SaleRefunds_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SaleRefundDetails] (
    [Id] int NOT NULL IDENTITY,
    [SaleRefundId] int NOT NULL,
    [SaleDetailId] int NOT NULL,
    [ProductId] int NOT NULL,
    [QuantityReturned] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_SaleRefundDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SaleRefundDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SaleRefundDetails_SaleDetails_SaleDetailId] FOREIGN KEY ([SaleDetailId]) REFERENCES [SaleDetails] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SaleRefundDetails_SaleRefunds_SaleRefundId] FOREIGN KEY ([SaleRefundId]) REFERENCES [SaleRefunds] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_SaleRefundDetails_ProductId] ON [SaleRefundDetails] ([ProductId]);
GO

CREATE INDEX [IX_SaleRefundDetails_SaleDetailId] ON [SaleRefundDetails] ([SaleDetailId]);
GO

CREATE INDEX [IX_SaleRefundDetails_SaleRefundId] ON [SaleRefundDetails] ([SaleRefundId]);
GO

CREATE INDEX [IX_SaleRefunds_CashRegisterId] ON [SaleRefunds] ([CashRegisterId]);
GO

CREATE INDEX [IX_SaleRefunds_SaleId] ON [SaleRefunds] ([SaleId]);
GO

CREATE INDEX [IX_SaleRefunds_UserId] ON [SaleRefunds] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260107042553_AddSaleRefundSystem', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260107055457_AddIsActiveToUser', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

UPDATE AspNetUsers SET IsActive = 1
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260107055818_SetExistingUsersActive', N'8.0.0');
GO

COMMIT;
GO

