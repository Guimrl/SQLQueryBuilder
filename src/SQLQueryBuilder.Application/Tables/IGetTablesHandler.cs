namespace SQLQueryBuilder.Application.Tables;

public interface IGetTablesHandler
{
    Task<GetTablesResponse> Handle(
        GetTablesCommand command,
        ITablesRepository tablesRepository,
        CancellationToken cancellationToken);
}
