using System.Text.Json;

namespace SQLQueryBuilder.Application.Queries;

public sealed record GenerateQueryCommand(
    string? Table,
    string[]? Columns,
    string? Alias = null,
    OrderByColumn[]? OrderBy = null,
    WhereCondition[]? Where = null);

public sealed record OrderByColumn(string? Column, string? Direction = null);

public sealed record WhereCondition(
    string? Column,
    WhereOperator? Operator,
    JsonElement? Value = null);

public enum WhereOperator
{
    Equal,
    NotEqual,
    Contains,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    IsNull,
    IsNotNull
}