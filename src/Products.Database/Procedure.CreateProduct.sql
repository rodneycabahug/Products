CREATE PROCEDURE [dbo].[CreateProduct]
	@Id UNIQUEIDENTIFIER = NULL,
	@Name VARCHAR(100),
	@Description VARCHAR(500) = NULL,
	@Price DECIMAL(18, 2),
	@DeliveryPrice DECIMAL(18, 2)
AS
BEGIN
	IF (@Id IS NULL)
		INSERT INTO [dbo].[Product]([Name], [Description], [Price], [DeliveryPrice])
		OUTPUT INSERTED.Id
		VALUES(@Name, @Description, @Price, @DeliveryPrice)
	ELSE
		INSERT INTO [dbo].[Product]([Id], [Name], [Description], [Price], [DeliveryPrice])
		OUTPUT INSERTED.Id
		VALUES(@Id, @Name, @Description, @Price, @DeliveryPrice)

	RETURN 0
END