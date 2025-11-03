using Products.Domain.Models;

namespace Products.Application.Services;

public interface IProductOptionService
{
    Task<IEnumerable<ProductOptionModel>> RetrieveAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductOptionModel>> RetrieveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<ProductOptionModel> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductOptionModel> CreateAsync(ProductOptionModel productOption, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, ProductOptionModel productOption, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
