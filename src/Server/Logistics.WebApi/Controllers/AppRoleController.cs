namespace Logistics.WebApi.Controllers;

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
    public async Task<IActionResult> GetList([FromQuery] GetAppRolesRequest request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
