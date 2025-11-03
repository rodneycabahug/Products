-- =============================================
-- Products API Database Setup Script
-- Target: SQL Server 2019+ / LocalDB / SQL Express
-- =============================================

USE master
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ProductsDB')
BEGIN
    CREATE DATABASE ProductsDB
    PRINT 'Database ProductsDB created successfully.'
END
ELSE
BEGIN
    PRINT 'Database ProductsDB already exists.'
END
GO

USE ProductsDB
GO

-- =============================================
-- Drop existing objects (for clean setup)
-- =============================================
PRINT 'Dropping existing stored procedures...'

IF OBJECT_ID('dbo.CreateProduct', 'P') IS NOT NULL DROP PROCEDURE dbo.CreateProduct
IF OBJECT_ID('dbo.CreateProductOption', 'P') IS NOT NULL DROP PROCEDURE dbo.CreateProductOption
IF OBJECT_ID('dbo.DeleteProduct', 'P') IS NOT NULL DROP PROCEDURE dbo.DeleteProduct
IF OBJECT_ID('dbo.DeleteProductOption', 'P') IS NOT NULL DROP PROCEDURE dbo.DeleteProductOption
IF OBJECT_ID('dbo.RetrieveProductById', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProductById
IF OBJECT_ID('dbo.RetrieveProductOptions', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProductOptions
IF OBJECT_ID('dbo.RetrieveProductOptionsById', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProductOptionsById
IF OBJECT_ID('dbo.RetrieveProductOptionsByProductId', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProductOptionsByProductId
IF OBJECT_ID('dbo.RetrieveProducts', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProducts
IF OBJECT_ID('dbo.RetrieveProductsByName', 'P') IS NOT NULL DROP PROCEDURE dbo.RetrieveProductsByName
IF OBJECT_ID('dbo.UpdateProduct', 'P') IS NOT NULL DROP PROCEDURE dbo.UpdateProduct
IF OBJECT_ID('dbo.UpdateProductOption', 'P') IS NOT NULL DROP PROCEDURE dbo.UpdateProductOption

PRINT 'Dropping existing tables...'

IF OBJECT_ID('dbo.ProductOption', 'U') IS NOT NULL DROP TABLE dbo.ProductOption
IF OBJECT_ID('dbo.Product', 'U') IS NOT NULL DROP TABLE dbo.Product

-- =============================================
-- Create Tables
-- =============================================
PRINT 'Creating Product table...'

CREATE TABLE [dbo].[Product] (
    [Id]            UNIQUEIDENTIFIER CONSTRAINT [DF_ProductId] DEFAULT (newsequentialid()) NOT NULL,
    [Name]          NVARCHAR (100)   NOT NULL,
    [Description]   NVARCHAR (500)   NULL,
    [Price]         DECIMAL (18, 2)  NOT NULL,
    [DeliveryPrice] DECIMAL (18, 2)  NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

PRINT 'Creating ProductOption table...'

CREATE TABLE [dbo].[ProductOption] (
    [Id]          UNIQUEIDENTIFIER CONSTRAINT [DF_ProductOptionId] DEFAULT (newsequentialid()) NOT NULL,
    [ProductId]   UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR (100)   NOT NULL,
    [Description] NVARCHAR (500)   NULL,
    CONSTRAINT [PK_ProductOption] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ProductOption_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
)
GO

-- =============================================
-- Create Stored Procedures - Product
-- =============================================
PRINT 'Creating stored procedures for Product...'

-- RetrieveProducts
CREATE PROCEDURE [dbo].[RetrieveProducts]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Description], [Price], [DeliveryPrice]
    FROM [dbo].[Product]
    ORDER BY [Name]
END
GO

-- RetrieveProductById
CREATE PROCEDURE [dbo].[RetrieveProductById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Description], [Price], [DeliveryPrice]
    FROM [dbo].[Product]
    WHERE [Id] = @Id
END
GO

-- RetrieveProductsByName
CREATE PROCEDURE [dbo].[RetrieveProductsByName]
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Description], [Price], [DeliveryPrice]
    FROM [dbo].[Product]
    WHERE [Name] LIKE '%' + @Name + '%'
    ORDER BY [Name]
END
GO

-- CreateProduct
CREATE PROCEDURE [dbo].[CreateProduct]
    @Id UNIQUEIDENTIFIER = NULL,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Price DECIMAL(18, 2),
    @DeliveryPrice DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF (@Id IS NULL)
        INSERT INTO [dbo].[Product]([Name], [Description], [Price], [DeliveryPrice])
        OUTPUT INSERTED.Id
        VALUES(@Name, @Description, @Price, @DeliveryPrice)
    ELSE
        INSERT INTO [dbo].[Product]([Id], [Name], [Description], [Price], [DeliveryPrice])
        OUTPUT INSERTED.Id
        VALUES(@Id, @Name, @Description, @Price, @DeliveryPrice)
END
GO

-- UpdateProduct
CREATE PROCEDURE [dbo].[UpdateProduct]
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Price DECIMAL(18, 2),
    @DeliveryPrice DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Product]
    SET [Name] = @Name,
        [Description] = @Description,
        [Price] = @Price,
        [DeliveryPrice] = @DeliveryPrice
    WHERE [Id] = @Id
END
GO

-- DeleteProduct
CREATE PROCEDURE [dbo].[DeleteProduct]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Product]
    WHERE [Id] = @Id
END
GO

-- =============================================
-- Create Stored Procedures - ProductOption
-- =============================================
PRINT 'Creating stored procedures for ProductOption...'

-- RetrieveProductOptions
CREATE PROCEDURE [dbo].[RetrieveProductOptions]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [ProductId], [Name], [Description]
    FROM [dbo].[ProductOption]
    ORDER BY [Name]
END
GO

-- RetrieveProductOptionsById
CREATE PROCEDURE [dbo].[RetrieveProductOptionsById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [ProductId], [Name], [Description]
    FROM [dbo].[ProductOption]
    WHERE [Id] = @Id
END
GO

-- RetrieveProductOptionsByProductId
CREATE PROCEDURE [dbo].[RetrieveProductOptionsByProductId]
    @ProductId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [ProductId], [Name], [Description]
    FROM [dbo].[ProductOption]
    WHERE [ProductId] = @ProductId
    ORDER BY [Name]
END
GO

-- CreateProductOption
CREATE PROCEDURE [dbo].[CreateProductOption]
    @Id UNIQUEIDENTIFIER = NULL,
    @ProductId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF (@Id IS NULL)
        INSERT INTO [dbo].[ProductOption]([ProductId], [Name], [Description])
        OUTPUT INSERTED.Id
        VALUES(@ProductId, @Name, @Description)
    ELSE
        INSERT INTO [dbo].[ProductOption]([Id], [ProductId], [Name], [Description])
        OUTPUT INSERTED.Id
        VALUES(@Id, @ProductId, @Name, @Description)
END
GO

-- UpdateProductOption
CREATE PROCEDURE [dbo].[UpdateProductOption]
    @Id UNIQUEIDENTIFIER,
    @ProductId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[ProductOption]
    SET [ProductId] = @ProductId,
        [Name] = @Name,
        [Description] = @Description
    WHERE [Id] = @Id
END
GO

-- DeleteProductOption
CREATE PROCEDURE [dbo].[DeleteProductOption]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[ProductOption]
    WHERE [Id] = @Id
END
GO

-- =============================================
-- Insert Sample Data (Optional)
-- =============================================
PRINT 'Inserting sample data...'

DECLARE @Product1Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Product2Id UNIQUEIDENTIFIER = NEWID()
DECLARE @Product3Id UNIQUEIDENTIFIER = NEWID()

-- Insert Products
INSERT INTO [dbo].[Product] ([Id], [Name], [Description], [Price], [DeliveryPrice])
VALUES 
    (@Product1Id, 'Samsung Galaxy S23', 'Latest Samsung flagship smartphone', 799.99, 9.99),
    (@Product2Id, 'Apple iPhone 15 Pro', 'Premium Apple smartphone with titanium design', 999.99, 9.99),
    (@Product3Id, 'Google Pixel 8', 'Google AI-powered smartphone', 699.99, 9.99)

-- Insert Product Options
INSERT INTO [dbo].[ProductOption] ([ProductId], [Name], [Description])
VALUES 
    (@Product1Id, '128GB Storage', 'Base storage option'),
    (@Product1Id, '256GB Storage', 'Double storage capacity'),
    (@Product1Id, 'Black Color', 'Phantom Black color variant'),
    (@Product1Id, 'White Color', 'Phantom White color variant'),
    (@Product2Id, '256GB Storage', 'Standard storage option'),
    (@Product2Id, '512GB Storage', 'Extended storage option'),
    (@Product2Id, 'Natural Titanium', 'Natural titanium finish'),
    (@Product2Id, 'Blue Titanium', 'Blue titanium finish'),
    (@Product3Id, '128GB Storage', 'Base storage option'),
    (@Product3Id, '256GB Storage', 'Extended storage option'),
    (@Product3Id, 'Obsidian', 'Black color variant'),
    (@Product3Id, 'Porcelain', 'White color variant')

-- =============================================
-- Verification
-- =============================================
PRINT ''
PRINT '============================================='
PRINT 'Database setup completed successfully!'
PRINT '============================================='
PRINT ''
PRINT 'Tables created:'
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME
PRINT ''
PRINT 'Stored procedures created:'
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY ROUTINE_NAME
PRINT ''
PRINT 'Sample data inserted:'
SELECT COUNT(*) AS ProductCount FROM [dbo].[Product]
SELECT COUNT(*) AS ProductOptionCount FROM [dbo].[ProductOption]
PRINT ''
PRINT 'Database is ready for use!'
GO
