namespace SQLQueryBuilder.Application.Queries;

public interface IGenerateQueryHandler
{
    Task<GenerateQueryResult> Handle(
        GenerateQueryCommand command,
        CancellationToken cancellationToken);
}