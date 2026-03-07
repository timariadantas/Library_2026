using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using NUlid;
using Oracle.ManagedDataAccess.Client;

namespace Library.Infrastructure.Oracle;

public class OracleLoanRepository : ILoanRepository
{
    private readonly string _connectionString;

    
    public OracleLoanRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Oracle")
        ?? throw new ArgumentNullException("Connection string 'Oracle' not found.");
    }
    private OracleConnection CreateConnection()
    => new OracleConnection(_connectionString);

    public void Create(Loan loan)
    {
        using var conn = CreateConnection();
        conn.Open();

        var sql = @"INSERT INTO loan (id, portfolio_id, user_id, start_at, period)
                     VALUES (:id, :portfolio_id, :user_id, :start_at, :period, )";
        using var cmd = new OracleCommand(sql, conn);

        cmd.Parameters.Add(new OracleParameter("id", loan.Id));
        cmd.Parameters.Add(new OracleParameter("portfolio_id", loan.PortfolioId));
        cmd.Parameters.Add(new OracleParameter("user_id", loan.UserId));
        cmd.Parameters.Add(new OracleParameter("start_at", loan.StartAt));
        cmd.Parameters.Add(new OracleParameter("period", loan.Period));



        cmd.ExecuteNonQuery();
    }

    public void Delete (Ulid id)
    {
        using var conn = CreateConnection();
        conn.Open();

        var sql = "DELETE FROM loan WHERE id = :id";

        using var cmd = new OracleCommand(sql , conn);

        cmd.Parameters.Add(new OracleParameter("id", id.ToString()));
        
        cmd.ExecuteNonQuery();
        
    }

    public Loan? GetById(Ulid id)
    {
        using var conn = CreateConnection();
        conn.Open();

        var sql = "SELECT * FROM loan WHERE id = :id";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("id", id.ToString()));
        
        using var reader = cmd.ExecuteReader();

        if(!reader.Read())
        return null;
         var loanId = Ulid.Parse(reader["id"].ToString());
        var portfolioId= Ulid.Parse(reader["portfolio_id"].ToString());
        var userId = Ulid.Parse(reader["user_id"].ToString());
        var startAt = Convert.ToDateTime(reader["start_at"]);
        var period = Convert.ToInt32(reader["period"]);
            var loanCondition = Enum.Parse<BookCondition>(reader["loan_condition"].ToString()!);
        

    DateTime? returnAt = reader["return_at"] != DBNull.Value
        ? Convert.ToDateTime(reader["return_at"])
        : null;

    BookCondition? returnCondition = reader["return_condition"] != DBNull.Value
        ? Enum.Parse<BookCondition>(reader["return_condition"].ToString()!)
        : null;
      
        return new Loan(
        loanId,
        portfolioId,
        userId,
        startAt,
        period,
        loanCondition,
        returnAt,
        returnCondition
    );
        

    }

  public IEnumerable<Loan> GetAll()
    {
        var loans = new List<Loan>();

        using var conn = CreateConnection();
        conn.Open();

        var sql = "SELECT * FROM loan";
        using var cmd = new OracleCommand(sql, conn);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var loanId = Ulid.Parse(reader["id"].ToString());
            var portfolioId = Ulid.Parse(reader["portfolio_id"].ToString());
            var userId = Ulid.Parse(reader["user_id"].ToString());
            var startAt = Convert.ToDateTime(reader["start_at"]);
            var period = Convert.ToInt32(reader["period"]);
            var loanCondition = Enum.Parse<BookCondition>(reader["loan_condition"].ToString()!);
            DateTime? returnAt = reader["return_at"] != DBNull.Value ? Convert.ToDateTime(reader["return_at"]) : null;
            BookCondition? returnCondition = reader["return_condition"] != DBNull.Value
                ? Enum.Parse<BookCondition>(reader["return_condition"].ToString()!)
                : null;

            loans.Add(new Loan(
                loanId,
                portfolioId,
                userId,
                startAt,
                period,
                loanCondition,
                returnAt,
                returnCondition
            ));
        }

        return loans;
    }

    public IEnumerable<Loan> GetByUserId(Ulid userId)
    {
        var loans = new List<Loan>();

        using var conn = CreateConnection();
        conn.Open();

        var sql = "SELECT * FROM loan WHERE user_id = :user_id";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("user_id", userId.ToString()));

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var loanId = Ulid.Parse(reader["id"].ToString());
            var portfolioId = Ulid.Parse(reader["portfolio_id"].ToString());
            var userIdDb = Ulid.Parse(reader["user_id"].ToString());
            var startAt = Convert.ToDateTime(reader["start_at"]);
            var period = Convert.ToInt32(reader["period"]);
            var loanCondition = Enum.Parse<BookCondition>(reader["loan_condition"].ToString()!);
            DateTime? returnAt = reader["return_at"] != DBNull.Value 
            ? Convert.ToDateTime(reader["return_at"]) : null;
            
            BookCondition? returnCondition = reader["return_condition"] != DBNull.Value
                ? Enum.Parse<BookCondition>(reader["return_condition"].ToString()!)
                : null;

            loans.Add(new Loan(
                loanId,
                portfolioId,
                userIdDb,
                startAt,
                period,
                loanCondition,
                returnAt,
                returnCondition
            ));
        }

        return loans;
    }

    public void UpdateReturn(Loan loan)
    {
        using var conn = CreateConnection();
        conn.Open();

        var sql = @"UPDATE loan 
                    SET return_at = :return_at,
                        return_condition = :return_condition
                    WHERE id = :id";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("return_at", loan.ReturnAt));
        cmd.Parameters.Add(new OracleParameter("return_condition", loan.ReturnCondition?.ToString()));
        cmd.Parameters.Add(new OracleParameter("id", loan.Id.ToString()));

        cmd.ExecuteNonQuery();
    }

} 
