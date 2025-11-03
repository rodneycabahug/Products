using TUnit.Core;
using Moq;
using Products.Application.Services;
using Products.Application.Exceptions;
using Products.Domain.Repositories;
using Products.Domain.Entities;
using Products.Domain.Models;

namespace Products.Application.Tests.Services;

public class ProductOptionServiceTests
{
    private Mock<IProductOptionRepository> _mockRepository = null!;
    private ProductOptionService _service = null!;

    [Before(Test)]
    public async Task Setup()
    {
        _mockRepository = new Mock<IProductOptionRepository>();
        _service = new ProductOptionService(_mockRepository.Object);
        await Task.CompletedTask;
    }

    [Test]
    public async Task RetrieveByProductIdAsync_ShouldReturnOptionsForProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var options = new List<ProductOptionEntity>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId, Name = "Option 1", Description = "Desc 1" },
            new() { Id = Guid.NewGuid(), ProductId = productId, Name = "Option 2", Description = null }
        };
        _mockRepository.Setup(r => r.RetrieveByProductIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        // Act
        var result = await _service.RetrieveByProductIdAsync(productId);

        // Assert
        await Assert.That(result.Count()).IsEqualTo(2);
        _mockRepository.Verify(r => r.RetrieveByProductIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RetrieveByIdAsync_WhenOptionExists_ShouldReturnOption()
    {
        // Arrange
        var optionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var option = new ProductOptionEntity
        {
            Id = optionId,
            ProductId = productId,
            Name = "Test Option",
            Description = "Test Description"
        };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(optionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(option);

        // Act
        var result = await _service.RetrieveByIdAsync(optionId);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsEqualTo(optionId);
        await Assert.That(result.ProductId).IsEqualTo(productId);
    }

    [Test]
    public async Task RetrieveByIdAsync_WhenOptionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var optionId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RetrieveByIdAsync(optionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductOptionEntity?)null);

        // Act & Assert
        await Assert.That(async () => await _service.RetrieveByIdAsync(optionId))
            .Throws<NotFoundException>();
    }

    [Test]
    public async Task CreateAsync_ShouldCreateOptionAndReturnModel()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var optionModel = new ProductOptionModel
        {
            Id = Guid.Empty,
            ProductId = productId,
            Name = "New Option",
            Description = "New Description"
        };
        var createdEntity = new ProductOptionEntity
        {
            Id = Guid.NewGuid(),
            ProductId = optionModel.ProductId,
            Name = optionModel.Name,
            Description = optionModel.Description
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<ProductOptionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(optionModel);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(result.Name).IsEqualTo(optionModel.Name);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<ProductOptionEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WhenOptionExists_ShouldUpdateOption()
    {
        // Arrange
        var optionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingOption = new ProductOptionEntity
        {
            Id = optionId,
            ProductId = productId,
            Name = "Old Name",
            Description = "Old Description"
        };
        var updatedModel = new ProductOptionModel
        {
            Id = optionId,
            ProductId = productId,
            Name = "New Name",
            Description = "New Description"
        };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(optionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOption);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ProductOptionEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(optionId, updatedModel);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<ProductOptionEntity>(e =>
            e.Id == optionId &&
            e.Name == "New Name"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_WhenOptionExists_ShouldDeleteOption()
    {
        // Arrange
        var optionId = Guid.NewGuid();
        var existingOption = new ProductOptionEntity
        {
            Id = optionId,
            ProductId = Guid.NewGuid(),
            Name = "Option",
            Description = null
        };
        _mockRepository.Setup(r => r.RetrieveByIdAsync(optionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOption);
        _mockRepository.Setup(r => r.DeleteAsync(optionId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(optionId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(optionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
