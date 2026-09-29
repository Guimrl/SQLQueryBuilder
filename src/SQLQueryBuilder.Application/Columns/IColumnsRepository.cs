namespace SQLQueryBuilder.Application.Columns;

public interface IColumnsRepository
{
    Task<IReadOnlyList<ColumnInfo>> GetColumnsAsync(
        string tableName,
        CancellationToken cancellationToken);
}
