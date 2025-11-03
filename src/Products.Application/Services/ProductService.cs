using Products.Application.Exceptions;
using Products.Application.Mappings;
using Products.Domain.Models;
using Products.Domain.Repositories;

namespace Products.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductModel>> RetrieveAsync(CancellationToken cancellationToken = default)
    {
        var productEntities = await _productRepository.RetrieveAsync(cancellationToken);
        return productEntities.ToModels();
    }

    public async Task<ProductModel> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productEntity = await _productRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productEntity == null)
        {
            throw new NotFoundException($"No product found with Id {id}.");
        }

        return productEntity.ToModel();
    }

    public async Task<IEnumerable<ProductModel>> RetrieveByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var productEntities = await _productRepository.RetrieveByNameAsync(name, cancellationToken);
        return productEntities.ToModels();
    }

    public async Task<ProductModel> CreateAsync(ProductModel product, CancellationToken cancellationToken = default)
    {
        var productEntity = product.ToEntity();
        productEntity = await _productRepository.CreateAsync(productEntity, cancellationToken);
        return productEntity.ToModel();
    }

    public async Task UpdateAsync(Guid id, ProductModel product, CancellationToken cancellationToken = default)
    {
        var productEntity = await _productRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productEntity == null)
        {
            throw new NotFoundException($"No product found with Id {id}.");
        }

        productEntity.Name = product.Name;
        productEntity.Description = product.Description;
        productEntity.Price = product.Price;
        productEntity.DeliveryPrice = product.DeliveryPrice;
        productEntity.Id = id;

        await _productRepository.UpdateAsync(productEntity, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var productEntity = await _productRepository.RetrieveByIdAsync(id, cancellationToken);
        if (productEntity == null)
        {
            throw new NotFoundException($"No product found with Id {id}.");
        }

        await _productRepository.DeleteAsync(id, cancellationToken);
    }
}
