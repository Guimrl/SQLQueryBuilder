namespace SQLQueryBuilder.Application.Tables;

public interface ITablesRepository
{
    Task<IReadOnlyList<string>> GetTablesAsync(
        GetTablesFilters filters,
        CancellationToken cancellationToken);
}
