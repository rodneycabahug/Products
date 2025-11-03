using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Mappings;
using Products.Application.Services;
using Products.API.Helpers;

namespace Products.API.Endpoints.v2;

public static class ProductOptionsEndpoints
{
    public static RouteGroupBuilder MapProductOptionsV2Endpoints(this IVersionedEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v{version:apiVersion}/products/{productId:guid}/options")
            .HasApiVersion(2.0)
            .WithTags("ProductOptions");

        group.MapGet("/", GetProductOptions)
            .WithName("GetProductOptionsV2")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetProductOptionById)
            .WithName("GetProductOptionByIdV2")
            .WithOpenApi();

        group.MapPost("/", CreateProductOption)
            .WithName("CreateProductOptionV2")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateProductOption)
            .WithName("UpdateProductOptionV2")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteProductOption)
            .WithName("DeleteProductOptionV2")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> GetProductOptions(
        [FromRoute] Guid productId,
        [FromServices] IProductOptionService productOptionService,
        CancellationToken cancellationToken)
    {
        var productOptions = await productOptionService.RetrieveByProductIdAsync(productId, cancellationToken);
        var productOptionDTOs = productOptions.ToDTOs();
        var collection = new CollectionDTO<ProductOptionDTO> { Items = productOptionDTOs };
        return Results.Ok(collection);
    }

    private static async Task<IResult> GetProductOptionById(
        [FromRoute] Guid productId,
        [FromRoute] Guid id,
        [FromServices] IProductOptionService productOptionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var productOption = await productOptionService.RetrieveByIdAsync(id, cancellationToken);
            if (productOption.ProductId != productId)
            {
                return Results.NotFound(new { message = $"Product option {id} does not belong to product {productId}." });
            }

            var productOptionDTO = productOption.ToDTO();
            return Results.Ok(productOptionDTO);
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> CreateProductOption(
        [FromRoute] Guid productId,
        [FromBody] ProductOptionDTO productOptionDTO,
        [FromServices] IProductOptionService productOptionService,
        CancellationToken cancellationToken)
    {
        var (isValid, errors) = ValidationHelper.ValidateObject(productOptionDTO);
        if (!isValid)
        {
            return Results.BadRequest(new { errors });
        }

        var productOptionModel = productOptionDTO.ToModel();
        productOptionModel.ProductId = productId;

        var createdProductOption = await productOptionService.CreateAsync(productOptionModel, cancellationToken);
        var createdDTO = createdProductOption.ToDTO();
        return Results.Created($"/api/v2/products/{productId}/options/{createdDTO.Id}", createdDTO);
    }

    private static async Task<IResult> UpdateProductOption(
        [FromRoute] Guid productId,
        [FromRoute] Guid id,
        [FromBody] ProductOptionDTO productOptionDTO,
        [FromServices] IProductOptionService productOptionService,
        CancellationToken cancellationToken)
    {
        var (isValid, errors) = ValidationHelper.ValidateObject(productOptionDTO);
        if (!isValid)
        {
            return Results.BadRequest(new { errors });
        }

        try
        {
            var productOptionModel = productOptionDTO.ToModel();
            productOptionModel.ProductId = productId;

            await productOptionService.UpdateAsync(id, productOptionModel, cancellationToken);
            return Results.NoContent();
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteProductOption(
        [FromRoute] Guid productId,
        [FromRoute] Guid id,
        [FromServices] IProductOptionService productOptionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var productOption = await productOptionService.RetrieveByIdAsync(id, cancellationToken);
            if (productOption.ProductId != productId)
            {
                return Results.NotFound(new { message = $"Product option {id} does not belong to product {productId}." });
            }

            await productOptionService.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
    }
}
