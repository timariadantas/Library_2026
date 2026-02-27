using Library.Infrastructure.Data;
using Npgsql; // biblioteca que permite conectar no PostgreSQL usando .NET

namespace Library.Infrastructure.Data;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}

// Implementação na infra
// Baixo acoplamento