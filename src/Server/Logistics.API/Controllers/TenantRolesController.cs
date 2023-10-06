using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("tenant-roles")]
[ApiController]
public class TenantRolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantRolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<TenantRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.TenantRole.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantRolesQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
