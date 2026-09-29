namespace SQLQueryBuilder.Application.Columns;

public class GetColumnsResponse
{
    public required ColumnInfo[] Columns { get; init; }
}
