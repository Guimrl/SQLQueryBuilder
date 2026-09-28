namespace SQLQueryBuilder.Application.Tables;

public sealed class GetTablesHandler(ITablesRepository tablesRepository)
{
    public Task<IReadOnlyList<string>> HandleAsync(
        GetTablesCommand command,
        CancellationToken cancellationToken)
    {
        return tablesRepository.GetTablesAsync(command.Filters, cancellationToken);
    }
}
