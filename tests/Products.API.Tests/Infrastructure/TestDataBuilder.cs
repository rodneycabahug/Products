using Products.Application.DTOs;

namespace Products.API.Tests.Infrastructure;

public static class TestDataBuilder
{
    public static ProductDTO CreateProductDTO(
        string name = "Test Product",
        string? description = "Test Description",
        decimal price = 99.99m,
        decimal deliveryPrice = 9.99m)
    {
        return new ProductDTO
        {
            Name = name,
            Description = description,
            Price = price,
            DeliveryPrice = deliveryPrice
        };
    }

    public static ProductOptionDTO CreateProductOptionDTO(
        Guid? productId = null,
        string name = "Test Option",
        string? description = "Test Option Description")
    {
        return new ProductOptionDTO
        {
            ProductId = productId ?? Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public static List<ProductDTO> CreateMultipleProducts(int count)
    {
        var products = new List<ProductDTO>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateProductDTO(
                name: $"Product {i}",
                description: $"Description for product {i}",
                price: 10m * i,
                deliveryPrice: 5m + i
            ));
        }
        return products;
    }

    public static List<ProductOptionDTO> CreateMultipleProductOptions(Guid productId, int count)
    {
        var options = new List<ProductOptionDTO>();
        for (int i = 1; i <= count; i++)
        {
            options.Add(CreateProductOptionDTO(
                productId: productId,
                name: $"Option {i}",
                description: $"Description for option {i}"
            ));
        }
        return options;
    }
}
