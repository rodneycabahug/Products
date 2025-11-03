CREATE PROCEDURE [dbo].[RetrieveProductById]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SELECT *
	FROM [dbo].[Product]
	WHERE [Id] = @Id

	RETURN 0
END
GO
