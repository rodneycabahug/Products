using Products.Domain.Entities;

namespace Products.Domain.Repositories;

public interface IProductRepository : IRepository<ProductEntity>
{
    Task<IEnumerable<ProductEntity>> RetrieveByNameAsync(string name, CancellationToken cancellationToken = default);
}
