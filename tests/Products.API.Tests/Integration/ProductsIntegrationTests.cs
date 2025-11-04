using System.Net;
using System.Net.Http.Json;
using Products.API.Tests.Infrastructure;
using Products.Application.DTOs;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.Core;

namespace Products.API.Tests.Integration;

[NotInParallel("DatabaseTests")]
public class ProductsIntegrationTests
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

    [Test]
    public async Task GetAllProducts_WhenNoProducts_ReturnsEmptyCollection()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Starting GetAllProducts_WhenNoProducts_ReturnsEmptyCollection test");

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).IsEmpty();
    }

    [Test]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var productDto = TestDataBuilder.CreateProductDTO(
            name: "New Product",
            description: "A brand new product",
            price: 149.99m,
            deliveryPrice: 19.99m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", productDto);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductDTO>();
        await Assert.That(createdProduct).IsNotNull();
        await Assert.That(createdProduct!.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(createdProduct.Name).IsEqualTo("New Product");
        await Assert.That(createdProduct.Description).IsEqualTo("A brand new product");
        await Assert.That(createdProduct.Price).IsEqualTo(149.99m);
        await Assert.That(createdProduct.DeliveryPrice).IsEqualTo(19.99m);

        // Verify Location header
        await Assert.That(response.Headers.Location).IsNotNull();
        await Assert.That(response.Headers.Location!.ToString()).Contains($"/api/v1/products/{createdProduct.Id}");
    }

    [Test]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange - name is required, so empty name should fail
        var invalidProduct = new
        {
            Name = "",
            Description = "Test",
            Price = 10.00m,
            DeliveryPrice = 5.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetProductById_WhenProductExists_ReturnsProduct()
    {
        // Arrange - create a product first
        var productDto = TestDataBuilder.CreateProductDTO(name: "Test Product");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", productDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDTO>();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{createdProduct!.Id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDTO>();
        await Assert.That(product).IsNotNull();
        await Assert.That(product!.Id).IsEqualTo(createdProduct.Id);
        await Assert.That(product.Name).IsEqualTo("Test Product");
    }

    [Test]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{nonExistentId}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateProduct_WhenProductExists_ReturnsNoContent()
    {
        // Arrange - create a product first
        var productDto = TestDataBuilder.CreateProductDTO(name: "Original Name");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", productDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDTO>();

        // Update the product
        var updatedProduct = TestDataBuilder.CreateProductDTO(
            name: "Updated Name",
            description: "Updated Description",
            price: 199.99m,
            deliveryPrice: 29.99m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{createdProduct!.Id}", updatedProduct);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/v1/products/{createdProduct.Id}");
        var retrievedProduct = await getResponse.Content.ReadFromJsonAsync<ProductDTO>();

        await Assert.That(retrievedProduct).IsNotNull();
        await Assert.That(retrievedProduct!.Name).IsEqualTo("Updated Name");
        await Assert.That(retrievedProduct.Description).IsEqualTo("Updated Description");
        await Assert.That(retrievedProduct.Price).IsEqualTo(199.99m);
        await Assert.That(retrievedProduct.DeliveryPrice).IsEqualTo(29.99m);
    }

    [Test]
    public async Task UpdateProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var productDto = TestDataBuilder.CreateProductDTO();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{nonExistentId}", productDto);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProduct_WhenProductExists_ReturnsNoContent()
    {
        // Arrange - create a product first
        var productDto = TestDataBuilder.CreateProductDTO();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", productDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDTO>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{createdProduct!.Id}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/v1/products/{createdProduct.Id}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{nonExistentId}");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SearchProducts_ByName_ReturnsMatchingProducts()
    {
        // Arrange - create multiple products
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Samsung TV"));
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Samsung Phone"));
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Apple iPhone"));

        // Act
        var response = await _client.GetAsync("/api/v1/products/search?name=Samsung");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).HasCount().EqualTo(2);
        await Assert.That(result.Items.All(p => p.Name.Contains("Samsung"))).IsTrue();
    }

    [Test]
    public async Task SearchProducts_WithoutNameParameter_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products/search");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetAllProducts_WithMultipleProducts_ReturnsAllProducts()
    {
        // Arrange - create multiple products
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Product 1"));
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Product 2"));
        await _client.PostAsJsonAsync("/api/v1/products", TestDataBuilder.CreateProductDTO(name: "Product 3"));

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CollectionDTO<ProductDTO>>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Items).HasCount().EqualTo(3);
    }

}
