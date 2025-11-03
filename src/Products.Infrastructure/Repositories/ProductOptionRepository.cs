using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Products.Infrastructure.Repositories;

public class ProductOptionRepository : IProductOptionRepository
{
    private readonly IConnectionManager _connectionManager;

    public ProductOptionRepository(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<IEnumerable<ProductOptionEntity>> RetrieveAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<ProductOptionEntity>();

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProductOptions", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("Id");
        var productIdOrdinal = reader.GetOrdinal("ProductId");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descriptionOrdinal = reader.GetOrdinal("Description");

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ProductOptionEntity
            {
                Id = reader.GetGuid(idOrdinal),
                ProductId = reader.GetGuid(productIdOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
            });
        }

        return result;
    }

    public async Task<ProductOptionEntity?> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ProductOptionEntity? result = null;

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProductOptionsById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var nameOrdinal = reader.GetOrdinal("Name");
            var descriptionOrdinal = reader.GetOrdinal("Description");

            result = new ProductOptionEntity
            {
                Id = reader.GetGuid(idOrdinal),
                ProductId = reader.GetGuid(productIdOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
            };
        }

        return result;
    }

    public async Task<IEnumerable<ProductOptionEntity>> RetrieveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = new List<ProductOptionEntity>();

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProductOptionsByProductId", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@ProductId", SqlDbType.UniqueIdentifier).Value = productId;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("Id");
        var productIdOrdinal = reader.GetOrdinal("ProductId");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descriptionOrdinal = reader.GetOrdinal("Description");

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ProductOptionEntity
            {
                Id = reader.GetGuid(idOrdinal),
                ProductId = reader.GetGuid(productIdOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal)
            });
        }

        return result;
    }

    public async Task<ProductOptionEntity> CreateAsync(ProductOptionEntity entity, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("CreateProductOption", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (entity.Id != Guid.Empty)
        {
            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entity.Id;
        }
        command.Parameters.Add("@ProductId", SqlDbType.UniqueIdentifier).Value = entity.ProductId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)entity.Description ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        var idResult = await command.ExecuteScalarAsync(cancellationToken);
        entity.Id = (Guid)idResult!;

        return entity;
    }

    public async Task UpdateAsync(ProductOptionEntity entity, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("UpdateProductOption", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entity.Id;
        command.Parameters.Add("@ProductId", SqlDbType.UniqueIdentifier).Value = entity.ProductId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)entity.Description ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("DeleteProductOption", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
