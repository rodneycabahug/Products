using Products.Application.Exceptions;
using Products.Application.Mappings;
using Products.Domain.Models;
using Products.Domain.Repositories;

namespace Products.Application.Services;

public class ProductOptionService : IProductOptionService
{
    private readonly IProductOptionRepository _productOptionRepository;

    public ProductOptionService(IProductOptionRepository productOptionRepository)
    {
        _productOptionRepository = productOptionRepository;
    }

    public async Task<IEnumerable<ProductOptionModel>> RetrieveAsync(CancellationToken cancellationToken = default)
    {
        var productOptionEntities = await _productOptionRepository.RetrieveAsync(cancellationToken);
        return productOptionEntities.ToModels();
    }

    public async Task<ProductOptionModel> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productOptionEntity = await _productOptionRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productOptionEntity == null)
        {
            throw new NotFoundException($"No product option found with Id {id}.");
        }

        return productOptionEntity.ToModel();
    }

    public async Task<IEnumerable<ProductOptionModel>> RetrieveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var productOptionEntities = await _productOptionRepository.RetrieveByProductIdAsync(productId, cancellationToken);
        return productOptionEntities.ToModels();
    }

    public async Task<ProductOptionModel> CreateAsync(ProductOptionModel productOption, CancellationToken cancellationToken = default)
    {
        var productOptionEntity = productOption.ToEntity();
        productOptionEntity = await _productOptionRepository.CreateAsync(productOptionEntity, cancellationToken);
        return productOptionEntity.ToModel();
    }

    public async Task UpdateAsync(Guid id, ProductOptionModel productOption, CancellationToken cancellationToken = default)
    {
        var productOptionEntity = await _productOptionRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productOptionEntity == null)
        {
            throw new NotFoundException($"No product option found with Id {id}.");
        }

        productOptionEntity.ProductId = productOption.ProductId;
        productOptionEntity.Name = productOption.Name;
        productOptionEntity.Description = productOption.Description;
        productOptionEntity.Id = id;

        await _productOptionRepository.UpdateAsync(productOptionEntity, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productOptionEntity = await _productOptionRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productOptionEntity == null)
        {
            throw new NotFoundException($"No product option found with Id {id}.");
        }

        await _productOptionRepository.DeleteAsync(id, cancellationToken);
    }
}
