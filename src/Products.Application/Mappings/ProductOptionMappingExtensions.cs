using Products.Application.DTOs;
using Products.Domain.Entities;
using Products.Domain.Models;

namespace Products.Application.Mappings;

public static class ProductOptionMappingExtensions
{
    // Entity to Model
    public static ProductOptionModel ToModel(this ProductOptionEntity entity)
    {
        return new ProductOptionModel
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    // Model to DTO
    public static ProductOptionDTO ToDTO(this ProductOptionModel model)
    {
        return new ProductOptionDTO
        {
            Id = model.Id,
            ProductId = model.ProductId,
            Name = model.Name,
            Description = model.Description
        };
    }

    // DTO to Model
    public static ProductOptionModel ToModel(this ProductOptionDTO dto)
    {
        return new ProductOptionModel
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    // Model to Entity
    public static ProductOptionEntity ToEntity(this ProductOptionModel model)
    {
        return new ProductOptionEntity
        {
            Id = model.Id,
            ProductId = model.ProductId,
            Name = model.Name,
            Description = model.Description
        };
    }

    // Collection mappings
    public static IEnumerable<ProductOptionModel> ToModels(this IEnumerable<ProductOptionEntity> entities)
    {
        return entities.Select(e => e.ToModel());
    }

    public static IEnumerable<ProductOptionDTO> ToDTOs(this IEnumerable<ProductOptionModel> models)
    {
        return models.Select(m => m.ToDTO());
    }
}
