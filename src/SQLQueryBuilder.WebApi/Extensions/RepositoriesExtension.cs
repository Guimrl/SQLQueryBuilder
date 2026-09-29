using Npgsql;
using SQLQueryBuilder.Application.Columns;
using SQLQueryBuilder.Application.Tables;
using SQLQueryBuilder.Infra.Repository;

namespace SQLQueryBuilder.WebApi.Extensions;

public static class RepositoriesExtension
{
    public static void AddRepositories(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");

        builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        builder.Services.AddScoped<ITablesRepository, TablesRepository>();
        builder.Services.AddScoped<IColumnsRepository, ColumnsRepository>();
    }
}
