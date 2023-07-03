namespace Logistics.API.Controllers;

[Route("[controller]")]
[ApiController]
public class AppRoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppRoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<AppRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRole.View)]
    public async Task<IActionResult> GetList([FromQuery] GetAppRolesQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
