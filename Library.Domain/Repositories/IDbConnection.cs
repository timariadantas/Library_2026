using Npgsql;

namespace Library.Domain.Repositories;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();


}


//Isso aplica: Dependency Inversion Principle
//O domínio define o que precisa.
//A infraestrutura implementa.