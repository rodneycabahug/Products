CREATE TABLE [dbo].[Product] (
    [Id]            UNIQUEIDENTIFIER CONSTRAINT [DF_ProductId] DEFAULT (newsequentialid()) NOT NULL,
    [Name]          NVARCHAR (100)   NOT NULL,
    [Description]   NVARCHAR (500)   NULL,
    [Price]         DECIMAL (18, 2)  NOT NULL,
    [DeliveryPrice] DECIMAL (18, 2)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
