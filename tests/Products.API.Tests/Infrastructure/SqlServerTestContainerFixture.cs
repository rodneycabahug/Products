using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;

namespace Products.API.Tests.Infrastructure;

public class SqlServerTestContainerFixture : IAsyncDisposable
{
    private readonly MsSqlContainer _msSqlContainer;
    private bool _isInitialized;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private Respawner? _respawner;
    private IntegrationTestWebAppFactory? _factory;

    public SqlServerTestContainerFixture()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .WithName($"products-test-{Guid.NewGuid():N}")
            .Build();
    }

    public string ConnectionString => _msSqlContainer.GetConnectionString();

    public IntegrationTestWebAppFactory Factory
    {
        get
        {
            if (_factory == null)
                throw new InvalidOperationException("Factory not initialized. Call InitializeAsync first.");
            return _factory;
        }
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _msSqlContainer.StartAsync();
        await InitializeDatabaseAsync();
        _isInitialized = true;
    }

    private async Task InitializeDatabaseAsync()
    {
        var connectionString = ConnectionString;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Create tables
        await ExecuteSqlAsync(connection, GetCreateProductTableSql());
        await ExecuteSqlAsync(connection, GetCreateProductOptionTableSql());

        // Create stored procedures
        await ExecuteSqlAsync(connection, GetCreateProductProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductsProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductByIdProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductsByNameProcedureSql());
        await ExecuteSqlAsync(connection, GetUpdateProductProcedureSql());
        await ExecuteSqlAsync(connection, GetDeleteProductProcedureSql());

        await ExecuteSqlAsync(connection, GetCreateProductOptionProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductOptionsProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductOptionByIdProcedureSql());
        await ExecuteSqlAsync(connection, GetRetrieveProductOptionsByProductIdProcedureSql());
        await ExecuteSqlAsync(connection, GetUpdateProductOptionProcedureSql());
        await ExecuteSqlAsync(connection, GetDeleteProductOptionProcedureSql());

        // Initialize Respawner for efficient database resets
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = Array.Empty<Respawn.Graph.Table>()
        });

        // Create a SINGLE shared WebApplicationFactory for all tests
        // This ensures all tests use the same API instance with the same connection pool
        _factory = new IntegrationTestWebAppFactory(ConnectionString);
    }

    private static async Task ExecuteSqlAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        // Use the initialization lock to ensure database operations are serialized
        await _initializeLock.WaitAsync();
        try
        {
            if (_respawner == null)
                throw new InvalidOperationException("Respawner not initialized. Call InitializeAsync first.");

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Resetting database with Respawn...");

            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Respawn efficiently resets the database by deleting data from all tables
            // while respecting foreign key constraints
            await _respawner.ResetAsync(connection);

            // Verify deletion
            await using var cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Product]", connection);
            var count = (int)await cmd.ExecuteScalarAsync();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Database reset complete. Product count: {count}");
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    #region SQL Scripts

    private static string GetCreateProductTableSql() => @"
        CREATE TABLE [dbo].[Product] (
            [Id]            UNIQUEIDENTIFIER CONSTRAINT [DF_ProductId] DEFAULT (newsequentialid()) NOT NULL,
            [Name]          NVARCHAR (100)   NOT NULL,
            [Description]   NVARCHAR (500)   NULL,
            [Price]         DECIMAL (18, 2)  NOT NULL,
            [DeliveryPrice] DECIMAL (18, 2)  NOT NULL,
            PRIMARY KEY CLUSTERED ([Id] ASC)
        );";

    private static string GetCreateProductOptionTableSql() => @"
        CREATE TABLE [dbo].[ProductOption] (
            [Id]          UNIQUEIDENTIFIER CONSTRAINT [DF_ProductOptionId] DEFAULT (newsequentialid()) NOT NULL,
            [ProductId]   UNIQUEIDENTIFIER NOT NULL,
            [Name]        NVARCHAR (100)   NOT NULL,
            [Description] NVARCHAR (500)   NULL,
            PRIMARY KEY CLUSTERED ([Id] ASC),
            CONSTRAINT [FK_ProductOption_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE
        );";

    private static string GetCreateProductProcedureSql() => @"
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
        END";

    private static string GetRetrieveProductsProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProducts]
        AS
        BEGIN
            SELECT *
            FROM [dbo].[Product]
        END";

    private static string GetRetrieveProductByIdProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProductById]
            @Id UNIQUEIDENTIFIER
        AS
        BEGIN
            SELECT *
            FROM [dbo].[Product]
            WHERE [Id] = @Id
        END";

    private static string GetRetrieveProductsByNameProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProductsByName]
            @Name VARCHAR(100)
        AS
        BEGIN
            SELECT *
            FROM [dbo].[Product]
            WHERE [Name] LIKE '%' + @Name + '%'
        END";

    private static string GetUpdateProductProcedureSql() => @"
        CREATE PROCEDURE [dbo].[UpdateProduct]
            @Id UNIQUEIDENTIFIER,
            @Name VARCHAR(100),
            @Description VARCHAR(500) = NULL,
            @Price DECIMAL(18, 2),
            @DeliveryPrice DECIMAL(18, 2)
        AS
        BEGIN
            UPDATE [dbo].[Product]
            SET [Name] = @Name,
                [Description] = @Description,
                [Price] = @Price,
                [DeliveryPrice] = @DeliveryPrice
            WHERE [Id] = @Id

            RETURN 0
        END";

    private static string GetDeleteProductProcedureSql() => @"
        CREATE PROCEDURE [dbo].[DeleteProduct]
            @Id UNIQUEIDENTIFIER
        AS
        BEGIN
            DELETE FROM [dbo].[Product]
            WHERE [Id] = @Id
        END";

    private static string GetCreateProductOptionProcedureSql() => @"
        CREATE PROCEDURE [dbo].[CreateProductOption]
            @Id UNIQUEIDENTIFIER = NULL,
            @ProductId UNIQUEIDENTIFIER,
            @Name VARCHAR(100),
            @Description VARCHAR(500) = NULL
        AS
        BEGIN
            IF (@Id IS NULL)
                INSERT INTO [dbo].[ProductOption]([ProductId], [Name], [Description])
                OUTPUT INSERTED.Id
                VALUES(@ProductId, @Name, @Description)
            ELSE
                INSERT INTO [dbo].[ProductOption]([Id], [ProductId], [Name], [Description])
                OUTPUT INSERTED.Id
                VALUES(@Id, @ProductId, @Name, @Description)

            RETURN 0
        END";

    private static string GetRetrieveProductOptionsProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProductOptions]
        AS
        BEGIN
            SELECT *
            FROM [dbo].[ProductOption]
        END";

    private static string GetRetrieveProductOptionByIdProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProductOptionsById]
            @Id UNIQUEIDENTIFIER
        AS
        BEGIN
            SELECT *
            FROM [dbo].[ProductOption]
            WHERE [Id] = @Id
        END";

    private static string GetRetrieveProductOptionsByProductIdProcedureSql() => @"
        CREATE PROCEDURE [dbo].[RetrieveProductOptionsByProductId]
            @ProductId UNIQUEIDENTIFIER
        AS
        BEGIN
            SELECT *
            FROM [dbo].[ProductOption]
            WHERE [ProductId] = @ProductId
        END";

    private static string GetUpdateProductOptionProcedureSql() => @"
        CREATE PROCEDURE [dbo].[UpdateProductOption]
            @Id UNIQUEIDENTIFIER,
            @ProductId UNIQUEIDENTIFIER,
            @Name VARCHAR(100),
            @Description VARCHAR(500) = NULL
        AS
        BEGIN
            UPDATE [dbo].[ProductOption]
            SET [ProductId] = @ProductId,
                [Name] = @Name,
                [Description] = @Description
            WHERE [Id] = @Id

            RETURN 0
        END";

    private static string GetDeleteProductOptionProcedureSql() => @"
        CREATE PROCEDURE [dbo].[DeleteProductOption]
            @Id UNIQUEIDENTIFIER
        AS
        BEGIN
            DELETE FROM [dbo].[ProductOption]
            WHERE [Id] = @Id
        END";

    #endregion

    public async ValueTask DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
        await _msSqlContainer.DisposeAsync();
    }
}
