using System.Text.Json;
using SQLQueryBuilder.Application.Columns;
using SQLQueryBuilder.Application.Tables;

namespace SQLQueryBuilder.Application.Queries;

public class GenerateQueryHandler(
    ITablesRepository tablesRepository,
    IColumnsRepository columnsRepository) : IGenerateQueryHandler
{
    public async Task<GenerateQueryResult> Handle(
        GenerateQueryCommand command,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);
        var tableName = command.Table!;
        var columnNames = command.Columns!;

        var tables = await tablesRepository.GetTablesAsync(new GetTablesFilters(), cancellationToken);
        if (!tables.Contains(tableName, StringComparer.Ordinal))
        {
            throw new ArgumentException("A tabela informada não existe.", nameof(command.Table));
        }

        var availableColumns = await columnsRepository.GetColumnsAsync(tableName, cancellationToken);
        var availableNames = availableColumns
            .Select(column => column.Name)
            .ToHashSet(StringComparer.Ordinal);

        ValidateAvailableColumns(columnNames, availableNames, nameof(command.Columns));

        var tableAlias = string.IsNullOrWhiteSpace(command.Alias)
            ? null
            : command.Alias;

        var columnsSql = columnNames.Length == 0
            ? "*"
            : string.Join(", ", columnNames.Select(columnName =>
                tableAlias is null
                    ? QuoteIdentifier(columnName)
                    : $"{tableAlias}.{QuoteIdentifier(columnName)}"));

        var orderByColumns = command.OrderBy ?? [];

        ValidateAvailableColumns(
            orderByColumns.Select(orderBy => orderBy.Column!).ToArray(),
            availableNames,
            nameof(command.OrderBy));

        var orderBySql = orderByColumns.Length == 0
            ? string.Empty
            : $" ORDER BY {string.Join(", ", orderByColumns.Select(orderBy =>
            {
                var columnSql = tableAlias is null
                    ? QuoteIdentifier(orderBy.Column!)
                    : $"{tableAlias}.{QuoteIdentifier(orderBy.Column!)}";
                var direction = string.IsNullOrWhiteSpace(orderBy.Direction)
                    ? "ASC"
                    : orderBy.Direction.ToUpperInvariant();
                return $"{columnSql} {direction}";
            }))}";

        var whereConditions = command.Where ?? [];

        ValidateAvailableColumns(
            whereConditions.Select(condition => condition.Column!).ToArray(),
            availableNames,
            nameof(command.Where));

        var whereSql = whereConditions.Length == 0
            ? string.Empty
            : $" WHERE {string.Join(" AND ", whereConditions.Select(condition =>
                FormatWhereCondition(condition, tableAlias)))}";

        var fromSql = tableAlias is null
            ? QuoteIdentifier(tableName)
            : $"{QuoteIdentifier(tableName)} {tableAlias}";

        var sql = $"SELECT {columnsSql} FROM {fromSql}{whereSql}{orderBySql};";

        return new GenerateQueryResult(sql);
    }

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"")}\"";

    private static string FormatWhereCondition(WhereCondition condition, string? tableAlias)
    {
        var columnSql = tableAlias is null
            ? QuoteIdentifier(condition.Column!)
            : $"{tableAlias}.{QuoteIdentifier(condition.Column!)}";
        var filterOperator = condition.Operator!.Value;

        if (filterOperator is WhereOperator.IsNull or WhereOperator.IsNotNull)
        {
            return $"{columnSql} {(filterOperator == WhereOperator.IsNull ? "IS NULL" : "IS NOT NULL")}";
        }

        var value = condition.Value!.Value;
        if (filterOperator == WhereOperator.Contains)
        {
            var searchValue = value.GetString()!;
            var needsEscape = searchValue.IndexOfAny(['%', '_']) >= 0;
            var pattern = $"%{(needsEscape ? EscapeLikePattern(searchValue) : searchValue)}%";
            var patternLiteral = QuoteStringLiteral(pattern);
            return $"{columnSql} ILIKE {patternLiteral}{(needsEscape ? " ESCAPE '!'" : string.Empty)}";
        }

        return $"{columnSql} {GetSqlOperator(filterOperator)} {FormatSqlValue(value)}";
    }

    private static string FormatSqlValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => QuoteStringLiteral(value.GetString()!),
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "TRUE",
        JsonValueKind.False => "FALSE",
        _ => throw new ArgumentException("O valor deve ser texto, número ou booleano.", nameof(value))
    };

    private static string GetSqlOperator(WhereOperator filterOperator) => filterOperator switch
    {
        WhereOperator.Equal => "=",
        WhereOperator.NotEqual => "<>",
        WhereOperator.LessThan => "<",
        WhereOperator.LessThanOrEqual => "<=",
        WhereOperator.GreaterThan => ">",
        WhereOperator.GreaterThanOrEqual => ">=",
        _ => throw new ArgumentOutOfRangeException(nameof(filterOperator))
    };

    private static string EscapeLikePattern(string value) =>
        value.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");

    private static string QuoteStringLiteral(string value) =>
        $"'{value.Replace("'", "''")}'";

    private static void ValidateAvailableColumns(
        IEnumerable<string> columnNames,
        HashSet<string> availableNames,
        string parameterName)
    {
        if (columnNames.Any(columnName => !availableNames.Contains(columnName)))
        {
            throw new ArgumentException("Uma ou mais colunas não existem na tabela.", parameterName);
        }
    }

    private static void ValidateWhereConditions(WhereCondition[]? conditions)
    {
        foreach (var condition in conditions ?? [])
        {
            if (condition is null ||
                string.IsNullOrWhiteSpace(condition.Column) ||
                condition.Operator is null ||
                !Enum.IsDefined(typeof(WhereOperator), condition.Operator.Value))
            {
                throw new ArgumentException("Cada filtro deve ter coluna e operador válidos.", nameof(conditions));
            }

            var filterOperator = condition.Operator.Value;
            var valueKind = condition.Value?.ValueKind ?? JsonValueKind.Undefined;
            var hasValue = valueKind is not (JsonValueKind.Null or JsonValueKind.Undefined);
            var requiresValue = filterOperator is not (WhereOperator.IsNull or WhereOperator.IsNotNull);

            if (hasValue != requiresValue ||
                (hasValue && valueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)) ||
                (filterOperator == WhereOperator.Contains && valueKind != JsonValueKind.String))
            {
                throw new ArgumentException("O operador selecionado não aceita esse valor.", nameof(conditions));
            }
        }
    }

    private static bool IsSafeUnquotedAlias(string alias) =>
        alias.Length > 0 &&
        (alias[0] == '_' || alias[0] is >= 'a' and <= 'z') &&
        alias.All(character =>
            character == '_' ||
            character is >= 'a' and <= 'z' ||
            character is >= '0' and <= '9');

    private static void ValidateCommand(GenerateQueryCommand command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.Table))
        {
            throw new ArgumentException("A tabela é obrigatória.", nameof(command.Table));
        }

        if (command.Columns is null || command.Columns.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("A lista de colunas não pode ser nula nem conter valores vazios.", nameof(command.Columns));
        }

        if (command.OrderBy?.Any(orderBy =>
                orderBy is null ||
                string.IsNullOrWhiteSpace(orderBy.Column) ||
                (!string.IsNullOrWhiteSpace(orderBy.Direction) &&
                 !orderBy.Direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) &&
                 !orderBy.Direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))) == true)
        {
            throw new ArgumentException("Cada ordenação deve ter uma coluna e direção ASC ou DESC.", nameof(command.OrderBy));
        }

        ValidateWhereConditions(command.Where);

        if (!string.IsNullOrWhiteSpace(command.Alias) && !IsSafeUnquotedAlias(command.Alias))
        {
            throw new ArgumentException("O alias deve ser um identificador simples em minúsculas.", nameof(command.Alias));
        }

    }
}
