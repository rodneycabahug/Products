using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Products.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IConnectionManager _connectionManager;

    public ProductRepository(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<IEnumerable<ProductEntity>> RetrieveAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<ProductEntity>();

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProducts", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("Id");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descriptionOrdinal = reader.GetOrdinal("Description");
        var priceOrdinal = reader.GetOrdinal("Price");
        var deliveryPriceOrdinal = reader.GetOrdinal("DeliveryPrice");

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ProductEntity
            {
                Id = reader.GetGuid(idOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Price = reader.GetDecimal(priceOrdinal),
                DeliveryPrice = reader.GetDecimal(deliveryPriceOrdinal)
            });
        }

        return result;
    }

    public async Task<ProductEntity?> RetrieveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ProductEntity? result = null;

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProductById", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var nameOrdinal = reader.GetOrdinal("Name");
            var descriptionOrdinal = reader.GetOrdinal("Description");
            var priceOrdinal = reader.GetOrdinal("Price");
            var deliveryPriceOrdinal = reader.GetOrdinal("DeliveryPrice");

            result = new ProductEntity
            {
                Id = reader.GetGuid(idOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Price = reader.GetDecimal(priceOrdinal),
                DeliveryPrice = reader.GetDecimal(deliveryPriceOrdinal)
            };
        }

        return result;
    }

    public async Task<IEnumerable<ProductEntity>> RetrieveByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var result = new List<ProductEntity>();

        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("RetrieveProductsByName", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("Id");
        var nameOrdinal = reader.GetOrdinal("Name");
        var descriptionOrdinal = reader.GetOrdinal("Description");
        var priceOrdinal = reader.GetOrdinal("Price");
        var deliveryPriceOrdinal = reader.GetOrdinal("DeliveryPrice");

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ProductEntity
            {
                Id = reader.GetGuid(idOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Price = reader.GetDecimal(priceOrdinal),
                DeliveryPrice = reader.GetDecimal(deliveryPriceOrdinal)
            });
        }

        return result;
    }

    public async Task<ProductEntity> CreateAsync(ProductEntity entity, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("CreateProduct", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (entity.Id != Guid.Empty)
        {
            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entity.Id;
        }
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)entity.Description ?? DBNull.Value;
        command.Parameters.Add("@Price", SqlDbType.Decimal).Value = entity.Price;
        command.Parameters.Add("@DeliveryPrice", SqlDbType.Decimal).Value = entity.DeliveryPrice;

        await connection.OpenAsync(cancellationToken);
        var idResult = await command.ExecuteScalarAsync(cancellationToken);
        entity.Id = (Guid)idResult!;

        return entity;
    }

    public async Task UpdateAsync(ProductEntity entity, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("UpdateProduct", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entity.Id;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = entity.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object?)entity.Description ?? DBNull.Value;
        command.Parameters.Add("@Price", SqlDbType.Decimal).Value = entity.Price;
        command.Parameters.Add("@DeliveryPrice", SqlDbType.Decimal).Value = entity.DeliveryPrice;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionManager.CreateConnection();
        await using var command = new SqlCommand("DeleteProduct", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
