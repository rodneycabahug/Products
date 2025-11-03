CREATE PROCEDURE [dbo].[DeleteProduct]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	DELETE
	FROM [dbo].[Product]
	WHERE [Id] = @Id

	RETURN 0
END
GO
