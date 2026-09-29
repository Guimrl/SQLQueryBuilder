using Npgsql;
using SQLQueryBuilder.Application.Columns;

namespace SQLQueryBuilder.Infra.Repository;

public class ColumnsRepository(NpgsqlDataSource dataSource) : IColumnsRepository
{
    public async Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var dbCommand = dataSource.CreateCommand("""
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = $1
            ORDER BY ordinal_position
            """);
        dbCommand.Parameters.AddWithValue(tableName);

        await using var reader = await dbCommand.ExecuteReaderAsync(cancellationToken);
        var columns = new List<ColumnInfo>();

        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new ColumnInfo(reader.GetString(0), reader.GetString(1)));
        }

        return columns;
    }
}
