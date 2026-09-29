using Microsoft.AspNetCore.Mvc;
using SQLQueryBuilder.Application.Columns;

namespace SQLQueryBuilder.WebApi.Controllers;

[ApiController]
[Route("Columns")]
public class ColumnsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetColumnsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetColumns(
        [FromQuery] GetColumnsCommand command,
        [FromServices] IGetColumnsHandler handler,
        [FromServices] IColumnsRepository columnsRepository,
        CancellationToken cancellationToken)
    {
        var columns = await handler.Handle(command, columnsRepository, cancellationToken);

        return Ok(columns);
    }
}
