using Logistics.Shared.Models;

namespace Logistics.API.Controllers;

[Route("app-roles")]
[ApiController]
public class AppRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppRolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<AppRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.AppRoles.View)]
    public async Task<IActionResult> GetList([FromQuery] GetAppRolesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
