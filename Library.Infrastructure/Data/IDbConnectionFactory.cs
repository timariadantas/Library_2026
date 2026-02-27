using Npgsql;

namespace Library.Infrastructure.Data;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}
// Centralizar criação de conexão
//Evitar repetir string de conexão
//Facilitar troca de banco depois