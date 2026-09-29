namespace SQLQueryBuilder.Application.Columns;

public class GetColumnsHandler : IGetColumnsHandler
{
    public async Task<GetColumnsResponse> Handle(
        GetColumnsCommand command,
        IColumnsRepository columnsRepository,
        CancellationToken cancellationToken)
    {
        var columns = await columnsRepository.GetColumnsAsync(command.TableName, cancellationToken);
        return new GetColumnsResponse
        {
            Columns = columns.ToArray()
        };
    }
}
