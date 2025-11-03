namespace Products.Domain.Repositories;

public interface IRepository<T>
{
    Task<IEnumerable<T>> RetrieveAsync(CancellationToken cancellationToken = default);

    Task<T?> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
