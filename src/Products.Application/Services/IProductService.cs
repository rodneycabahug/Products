using Products.Domain.Models;

namespace Products.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductModel>> RetrieveAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductModel>> RetrieveByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<ProductModel> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductModel> CreateAsync(ProductModel product, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, ProductModel product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
