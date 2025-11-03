using Moq;
using Products.Domain.Entities;
using Products.Infrastructure.Data;
using Products.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using TUnit.Core;

namespace Products.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for ProductRepository.
/// Note: These tests demonstrate the pattern but require a real database connection to run.
/// In a real scenario, you would use a test database or Testcontainers.
/// </summary>
public class ProductRepositoryTests
{
    [Test]
    public async Task ConnectionManager_CreateConnection_ReturnsValidConnection()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=ProductsDB;User Id=sa;Password=Test@123;TrustServerCertificate=true";
        var connectionManager = new ConnectionManager(connectionString);

        // Act
        var connection = connectionManager.CreateConnection();

        // Assert
        await Assert.That(connection).IsNotNull();
        await Assert.That(connection).IsTypeOf<SqlConnection>();
        await Assert.That(connection.ConnectionString).IsEqualTo(connectionString);
    }

    [Test]
    public async Task ConnectionManager_CreatesNewConnectionEachTime()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=ProductsDB;User Id=sa;Password=Test@123;TrustServerCertificate=true";
        var connectionManager = new ConnectionManager(connectionString);

        // Act
        var connection1 = connectionManager.CreateConnection();
        var connection2 = connectionManager.CreateConnection();

        // Assert
        await Assert.That(connection1).IsNotNull();
        await Assert.That(connection2).IsNotNull();
        await Assert.That(connection1).IsNotSameReferenceAs(connection2);
    }

    /// <summary>
    /// This is a demonstration test that shows how you would test repository methods.
    /// In a real scenario, this would connect to a test database.
    /// For now, this test verifies the repository is properly constructed.
    /// </summary>
    [Test]
    public async Task ProductRepository_Constructor_CreatesInstance()
    {
        // Arrange
        var mockConnectionManager = new Mock<IConnectionManager>();
        
        // Act
        var repository = new ProductRepository(mockConnectionManager.Object);

        // Assert
        await Assert.That(repository).IsNotNull();
        await Assert.That(repository).IsTypeOf<ProductRepository>();
    }

    /// <summary>
    /// Example of how to structure an integration test with a real database.
    /// This test is skipped by default and would need proper database setup.
    /// </summary>
    [Test]
    [Skip("Requires test database - demonstration only")]
    public async Task ProductRepository_CreateAsync_InsertsProductToDatabase()
    {
        // Arrange
        // In a real test, you would:
        // 1. Set up a test database (using Testcontainers or similar)
        // 2. Create a connection manager with the test connection string
        // 3. Create the repository
        
        var connectionString = "Server=localhost;Database=ProductsDB_Test;User Id=sa;Password=Test@123;TrustServerCertificate=true";
        var connectionManager = new ConnectionManager(connectionString);
        var repository = new ProductRepository(connectionManager);

        var newProduct = new ProductEntity
        {
            Name = "Integration Test Product",
            Description = "This is a test",
            Price = 99.99m,
            DeliveryPrice = 5.99m
        };

        // Act
        var result = await repository.CreateAsync(newProduct);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(result.Name).IsEqualTo("Integration Test Product");

        // Cleanup
        await repository.DeleteAsync(result.Id);
    }
}
