using Microsoft.AspNetCore.Mvc;
using SQLQueryBuilder.Application.Tables;

namespace SQLQueryBuilder.WebApi.Controllers;

[ApiController]
[Route("Tables")]
public class TablesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetTablesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTables(
        [FromQuery] GetTablesCommand command,
        [FromServices] IGetTablesHandler handler,
        [FromServices] ITablesRepository tablesRepository,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, tablesRepository, cancellationToken);

        return Ok(result);
    }
}
