using Npgsql;
using SQLQueryBuilder.Application.Tables;

namespace SQLQueryBuilder.Infra.Repository;

public class TablesRepository(NpgsqlDataSource dataSource) : ITablesRepository
{
    public async Task<IReadOnlyList<string>> GetTablesAsync(
        GetTablesFilters filters,
        CancellationToken cancellationToken)
    {
        await using var dbCommand = dataSource.CreateCommand("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
            ORDER BY table_name
            """);

        await using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken);
        var tables = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
