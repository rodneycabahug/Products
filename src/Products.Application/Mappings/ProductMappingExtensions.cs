using Products.Application.DTOs;
using Products.Domain.Entities;
using Products.Domain.Models;

namespace Products.Application.Mappings;

public static class ProductMappingExtensions
{
    // Entity to Model
    public static ProductModel ToModel(this ProductEntity entity)
    {
        return new ProductModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            DeliveryPrice = entity.DeliveryPrice
        };
    }

    // Model to DTO
    public static ProductDTO ToDTO(this ProductModel model)
    {
        return new ProductDTO
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            DeliveryPrice = model.DeliveryPrice
        };
    }

    // DTO to Model
    public static ProductModel ToModel(this ProductDTO dto)
    {
        return new ProductModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DeliveryPrice = dto.DeliveryPrice
        };
    }

    // Model to Entity
    public static ProductEntity ToEntity(this ProductModel model)
    {
        return new ProductEntity
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            DeliveryPrice = model.DeliveryPrice
        };
    }

    // Collection mappings
    public static IEnumerable<ProductModel> ToModels(this IEnumerable<ProductEntity> entities)
    {
        return entities.Select(e => e.ToModel());
    }

    public static IEnumerable<ProductDTO> ToDTOs(this IEnumerable<ProductModel> models)
    {
        return models.Select(m => m.ToDTO());
    }
}
