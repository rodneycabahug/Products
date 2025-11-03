CREATE PROCEDURE [dbo].[RetrieveProductOptionsById]
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SELECT *
	FROM [dbo].[ProductOption]
	WHERE [Id] = @Id
	
	RETURN 0
END
GO
