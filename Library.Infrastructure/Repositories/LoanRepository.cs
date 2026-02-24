using Library.Domain.Interfaces;
using Library.Domain.Repositories;
using Npgsql;

namespace Library.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LoanRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public bool ExistsActiveLoan(string isbn)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();

        var sql = """
            SELECT 1
            FROM loans
            WHERE isbn = @isbn
            AND returned_at IS NULL;
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("isbn", isbn);

        return cmd.ExecuteScalar() != null;
    }
}
