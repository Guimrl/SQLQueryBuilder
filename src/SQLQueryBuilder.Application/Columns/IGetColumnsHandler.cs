namespace SQLQueryBuilder.Application.Columns;

public interface IGetColumnsHandler
{
    Task<GetColumnsResponse> Handle(
        GetColumnsCommand command,
        IColumnsRepository columnsRepository,
        CancellationToken cancellationToken);
}
