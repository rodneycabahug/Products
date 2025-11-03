CREATE PROCEDURE [dbo].[DeleteProductOption]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	DELETE
	FROM [dbo].[ProductOption]
	WHERE [Id] = @Id

	RETURN 0
END
GO
