using Microsoft.AspNetCore.Mvc;
using SQLQueryBuilder.Application.Tables;

namespace SQLQueryBuilder.WebApi.Controllers;

[ApiController]
[Route("Tables")]
public sealed class TablesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTables(
        [FromServices] GetTablesHandler handler,
        [FromQuery] GetTablesFilters filters,
        CancellationToken cancellationToken)
    {
        var command = new GetTablesCommand(filters);
        var tables = await handler.HandleAsync(command, cancellationToken);

        return Ok(tables);
    }
}
