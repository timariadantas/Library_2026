using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Library.Domain.Repositories;
using Library.Infrastructure.Postgres;
using Library.Infrastructure.Oracle;


namespace Library.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseProvider = configuration["Storage:Provider"];

        switch (databaseProvider)
        {
            case "Postgres":
                services.AddScoped<IBookRepository, PostgresBookRepository>();
                break;

            case "Oracle":
                services.AddScoped<IBookRepository, OracleBookRepository>();
                break;

         

            default:
                throw new Exception("Database provider not configured.");
        }

        return services;
    }
}