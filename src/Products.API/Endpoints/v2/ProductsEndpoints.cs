using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Mappings;
using Products.Application.Services;
using Products.API.Helpers;

namespace Products.API.Endpoints.v2;

public static class ProductsEndpoints
{
    public static RouteGroupBuilder MapProductsV2Endpoints(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v{version:apiVersion}/products")
            .HasApiVersion(2.0)
            .WithTags("Products");

        group.MapGet("/", GetAllProducts)
            .WithName("GetAllProductsV2")
            .WithOpenApi();

        group.MapGet("/search", SearchProducts)
            .WithName("SearchProductsV2")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductByIdV2")
            .WithOpenApi();

        group.MapPost("/", CreateProduct)
            .WithName("CreateProductV2")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProductV2")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProductV2")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> GetAllProducts(
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        var products = await productService.RetrieveAsync(cancellationToken);
        var productDTOs = products.ToDTOs();
        var collection = new CollectionDTO<ProductDTO> { Items = productDTOs };
        return Results.Ok(collection);
    }

    private static async Task<IResult> SearchProducts(
        [FromQuery] string? name,
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Results.BadRequest("Name parameter is required.");
        }

        var products = await productService.RetrieveByNameAsync(name, cancellationToken);
        var productDTOs = products.ToDTOs();
        var collection = new CollectionDTO<ProductDTO> { Items = productDTOs };
        return Results.Ok(collection);
    }

    private static async Task<IResult> GetProductById(
        [FromRoute] Guid id,
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.RetrieveByIdAsync(id, cancellationToken);
            var productDTO = product.ToDTO();
            return Results.Ok(productDTO);
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> CreateProduct(
        [FromBody] ProductDTO productDTO,
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        var (isValid, errors) = ValidationHelper.ValidateObject(productDTO);
        if (!isValid)
        {
            return Results.BadRequest(new { errors });
        }

        var productModel = productDTO.ToModel();
        var createdProduct = await productService.CreateAsync(productModel, cancellationToken);
        var createdDTO = createdProduct.ToDTO();
        return Results.Created($"/api/v2/products/{createdDTO.Id}", createdDTO);
    }

    private static async Task<IResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] ProductDTO productDTO,
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        var (isValid, errors) = ValidationHelper.ValidateObject(productDTO);
        if (!isValid)
        {
            return Results.BadRequest(new { errors });
        }

        try
        {
            var productModel = productDTO.ToModel();
            await productService.UpdateAsync(id, productModel, cancellationToken);
            return Results.NoContent();
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteProduct(
        [FromRoute] Guid id,
        [FromServices] IProductService productService,
        CancellationToken cancellationToken)
    {
        try
        {
            await productService.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }
}
