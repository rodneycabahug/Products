CREATE PROCEDURE [dbo].[RetrieveProductOptionsByProductId]
	@ProductId UNIQUEIDENTIFIER
AS
BEGIN
	SELECT *
	FROM [dbo].[ProductOption]
	WHERE [ProductId] = @ProductId
	
	RETURN 0
END
GO
