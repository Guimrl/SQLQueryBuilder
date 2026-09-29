namespace SQLQueryBuilder.Application.Tables;

public record GetTablesCommand
{
    public GetTablesFilters Filters { get; init; } = new();
}
