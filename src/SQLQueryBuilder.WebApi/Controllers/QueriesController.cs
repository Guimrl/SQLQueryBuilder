using Microsoft.AspNetCore.Mvc;
using SQLQueryBuilder.Application.Queries;

namespace SQLQueryBuilder.WebApi.Controllers;

[ApiController]
[Route("Queries")]
public class QueriesController : ControllerBase
{
    [HttpPost("Generate")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> Generate(
        [FromBody] GenerateQueryCommand command,
        [FromServices] IGenerateQueryHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(command, cancellationToken);
            return Ok(result.Sql!);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [exception.ParamName ?? "query"] = [exception.Message]
            }));
        }
    }
}
