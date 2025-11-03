using TUnit.Core;
using Moq;
using Products.Application.Services;
using Products.Application.Exceptions;
using Products.Domain.Repositories;
using Products.Domain.Entities;
using Products.Domain.Models;

namespace Products.Application.Tests.Services;

public class ProductServiceTests
{
    private Mock<IProductRepository> _mockRepository = null!;
    private ProductService _service = null!;

    [Before(Test)]
    public async Task Setup()
    {
        _mockRepository = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepository.Object);
        await Task.CompletedTask;
    }

    [Test]
    public async Task RetrieveAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Description = "Desc 1", Price = 10m, DeliveryPrice = 2m },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Description = "Desc 2", Price = 20m, DeliveryPrice = 3m }
        };
        _mockRepository.Setup(r => r.RetrieveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _service.RetrieveAsync();

        // Assert
        await Assert.That(result.Count()).IsEqualTo(2);
        _mockRepository.Verify(r => r.RetrieveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RetrieveByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            DeliveryPrice = 5.99m
        };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.RetrieveByIdAsync(productId);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsEqualTo(productId);
        await Assert.That(result.Name).IsEqualTo("Test Product");
    }

    [Test]
    public async Task RetrieveByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RetrieveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act & Assert
        await Assert.That(async () => await _service.RetrieveByIdAsync(productId))
            .Throws<NotFoundException>();
    }

    [Test]
    public async Task CreateAsync_ShouldCreateProductAndReturnModel()
    {
        // Arrange
        var productModel = new ProductModel
        {
            Id = Guid.Empty,
            Name = "New Product",
            Description = "New Description",
            Price = 50m,
            DeliveryPrice = 5m
        };
        var createdEntity = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = productModel.Name,
            Description = productModel.Description,
            Price = productModel.Price,
            DeliveryPrice = productModel.DeliveryPrice
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(productModel);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(result.Name).IsEqualTo(productModel.Name);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductEntity
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Price = 100m,
            DeliveryPrice = 10m
        };
        var updatedModel = new ProductModel
        {
            Id = productId,
            Name = "New Name",
            Description = "New Description",
            Price = 150m,
            DeliveryPrice = 15m
        };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(productId, updatedModel);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<ProductEntity>(e =>
            e.Id == productId &&
            e.Name == "New Name" &&
            e.Price == 150m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenProductExists_ShouldDeleteProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new ProductEntity { Id = productId, Name = "Product", Description = null, Price = 10m, DeliveryPrice = 2m };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(productId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RetrieveByNameAsync_ShouldReturnMatchingProducts()
    {
        // Arrange
        var searchName = "Test";
        var products = new List<ProductEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test Product 1", Description = null, Price = 10m, DeliveryPrice = 2m },
            new() { Id = Guid.NewGuid(), Name = "Test Product 2", Description = null, Price = 20m, DeliveryPrice = 3m }
        };
        _mockRepository.Setup(r => r.RetrieveByNameAsync(searchName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _service.RetrieveByNameAsync(searchName);

        // Assert
        await Assert.That(result.Count()).IsEqualTo(2);
        _mockRepository.Verify(r => r.RetrieveByNameAsync(searchName, It.IsAny<CancellationToken>()), Times.Once);
    }
}
