using Products.Domain.Entities;

namespace Products.Domain.Repositories;

public interface IProductOptionRepository : IRepository<ProductOptionEntity>
{
    Task<IEnumerable<ProductOptionEntity>> RetrieveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
