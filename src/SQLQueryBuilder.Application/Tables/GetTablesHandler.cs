namespace SQLQueryBuilder.Application.Tables;

public class GetTablesHandler : IGetTablesHandler
{
    public async Task<GetTablesResponse> Handle(
        GetTablesCommand command,
        ITablesRepository tablesRepository,
        CancellationToken cancellationToken)
    {
        var tables = await tablesRepository.GetTablesAsync(command.Filters, cancellationToken);
        return new GetTablesResponse
        {
            Name = tables.Select(name => name).ToArray()
        };
    }
}
