CREATE PROCEDURE [dbo].[UpdateProductOption]
	@Id UNIQUEIDENTIFIER,
	@ProductId UNIQUEIDENTIFIER,
	@Name VARCHAR(100),
	@Description VARCHAR(500) = NULL
AS
BEGIN
	UPDATE [dbo].[ProductOption]
	SET [ProductId]=@ProductId,
		[Name]=@Name,
		[Description]=@Description
	WHERE [Id]=@Id

	RETURN 0
END