using Microsoft.Data.SqlClient;

namespace Products.Infrastructure.Data;

public interface IConnectionManager
{
    SqlConnection CreateConnection();
}

public class ConnectionManager : IConnectionManager
{
    private readonly string _connectionString;

    public ConnectionManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
