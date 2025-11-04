using System.Net;
using System.Net.Http.Json;
using Products.API.Tests.Infrastructure;
using Products.Application.DTOs;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace Products.API.Tests.Integration;

[NotInParallel("DatabaseTests")]
public class ProductOptionsIntegrationTests
{
    private SqlServerTestContainerFixture _fixture = null!;
    private HttpClient _client = null!;

    [Before(Test)]
    public async Task TestSetup()
    {
        // Acquire global test lock to ensure only one test runs at a time
        await SharedTestFixture.AcquireTestLockAsync();

        // Get shared fixture instance
        _fixture = await SharedTestFixture.GetInstanceAsync();

        // Reset database BEFORE each test to ensure clean state
        await _fixture.ResetDatabaseAsync();

        // Use the SHARED factory for all tests - this ensures consistent connection pooling
        _client = _fixture.Factory.CreateClient();
    }

    [After(Test)]
    public async Task CleanupAfterTest()
    {
        _client?.Dispose();

        // Wait to ensure all async HTTP operations have completed
        // This is necessary because tests run in parallel and we need to ensure
        // this test's database operations are fully done before releasing the lock
        await Task.Delay(500);

        // Release global test lock
        SharedTestFixture.ReleaseTestLock();
    }

    private async Task<Guid> CreateTestProductAsync()
    {
        var productDto = TestDataBuilder.CreateProductDTO(name: "Test Product for Options");
        var response = await _client.PostAsJsonAsync("/api/v1/products", productDto);
        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDTO>();
        return createdProduct!.Id;
    }

    [Test]
    public async Task GetProductOptions_WhenNoOptions_ReturnsEmptyCollection()
    {
        // Arrange
        var productId = await CreateTestProductAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}/options");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductOptionDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).IsEmpty();
    }

    [Test]
    public async Task CreateProductOption_WithValidData_ReturnsCreatedOption()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var optionDto = TestDataBuilder.CreateProductOptionDTO(
            productId: productId,
            name: "Color: Red",
            description: "Red color option"
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options", optionDto);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var createdOption = await response.Content.ReadFromJsonAsync<ProductOptionDTO>();
        await Assert.That(createdOption).IsNotNull();
        await Assert.That(createdOption!.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(createdOption.ProductId).IsEqualTo(productId);
        await Assert.That(createdOption.Name).IsEqualTo("Color: Red");
        await Assert.That(createdOption.Description).IsEqualTo("Red color option");

        // Verify Location header
        await Assert.That(response.Headers.Location).IsNotNull();
        await Assert.That(response.Headers.Location!.ToString())
            .Contains($"/api/v1/products/{productId}/options/{createdOption.Id}");
    }

    [Test]
    public async Task CreateProductOption_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var invalidOption = new
        {
            ProductId = productId,
            Name = "", // Empty name should fail validation
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options", invalidOption);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetProductOptionById_WhenOptionExists_ReturnsOption()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var optionDto = TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Size: Large");
        var createResponse = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options", optionDto);
        var createdOption = await createResponse.Content.ReadFromJsonAsync<ProductOptionDTO>();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}/options/{createdOption!.Id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var option = await response.Content.ReadFromJsonAsync<ProductOptionDTO>();
        await Assert.That(option).IsNotNull();
        await Assert.That(option!.Id).IsEqualTo(createdOption.Id);
        await Assert.That(option.ProductId).IsEqualTo(productId);
        await Assert.That(option.Name).IsEqualTo("Size: Large");
    }

    [Test]
    public async Task GetProductOptionById_WhenOptionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}/options/{nonExistentId}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateProductOption_WhenOptionExists_ReturnsNoContent()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var optionDto = TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Original Option");
        var createResponse = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options", optionDto);
        var createdOption = await createResponse.Content.ReadFromJsonAsync<ProductOptionDTO>();

        var updatedOption = TestDataBuilder.CreateProductOptionDTO(
            productId: productId,
            name: "Updated Option",
            description: "Updated Description"
        );

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/products/{productId}/options/{createdOption!.Id}",
            updatedOption
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/v1/products/{productId}/options/{createdOption.Id}");
        var retrievedOption = await getResponse.Content.ReadFromJsonAsync<ProductOptionDTO>();

        await Assert.That(retrievedOption).IsNotNull();
        await Assert.That(retrievedOption!.Name).IsEqualTo("Updated Option");
        await Assert.That(retrievedOption.Description).IsEqualTo("Updated Description");
    }

    [Test]
    public async Task UpdateProductOption_WhenOptionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var nonExistentId = Guid.NewGuid();
        var optionDto = TestDataBuilder.CreateProductOptionDTO(productId: productId);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/products/{productId}/options/{nonExistentId}",
            optionDto
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProductOption_WhenOptionExists_ReturnsNoContent()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var optionDto = TestDataBuilder.CreateProductOptionDTO(productId: productId);
        var createResponse = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options", optionDto);
        var createdOption = await createResponse.Content.ReadFromJsonAsync<ProductOptionDTO>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{productId}/options/{createdOption!.Id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/v1/products/{productId}/options/{createdOption.Id}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProductOption_WhenOptionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = await CreateTestProductAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{productId}/options/{nonExistentId}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetProductOptions_WithMultipleOptions_ReturnsAllOptions()
    {
        // Arrange
        var productId = await CreateTestProductAsync();

        // Create multiple options
        await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options",
            TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Color: Red"));
        await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options",
            TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Size: Large"));
        await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options",
            TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Material: Cotton"));

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}/options");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductOptionDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).HasCount().EqualTo(3);
        await Assert.That(result.Items.All(o => o.ProductId == productId)).IsTrue();
    }

    [Test]
    public async Task DeleteProduct_WithOptions_DeletesAllOptions()
    {
        // Arrange
        var productId = await CreateTestProductAsync();

        // Create options
        var createResponse1 = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options",
            TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Option 1"));
        var option1 = await createResponse1.Content.ReadFromJsonAsync<ProductOptionDTO>();

        var createResponse2 = await _client.PostAsJsonAsync($"/api/v1/products/{productId}/options",
            TestDataBuilder.CreateProductOptionDTO(productId: productId, name: "Option 2"));
        var option2 = await createResponse2.Content.ReadFromJsonAsync<ProductOptionDTO>();

        // Act - delete the product
        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{productId}");

        // Assert
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify options are also deleted (cascade delete)
        var getOption1Response = await _client.GetAsync($"/api/v1/products/{productId}/options/{option1!.Id}");
        await Assert.That(getOption1Response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

        var getOption2Response = await _client.GetAsync($"/api/v1/products/{productId}/options/{option2!.Id}");
        await Assert.That(getOption2Response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetProductOptions_ForNonExistentProduct_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{nonExistentProductId}/options");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductOptionDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).IsEmpty();
    }

}
