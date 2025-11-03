CREATE PROCEDURE [dbo].[RetrieveProductsByName]
	@Name VARCHAR(100)
AS
BEGIN
	SET @Name = LOWER(@Name)

	SELECT *
	FROM [dbo].[Product]
	WHERE LOWER([Name]) like '%' + @Name + '%'

	RETURN 0
END
GO