using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("[controller]")]
[ApiController]
public class TenantRoleController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantRoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedResponseResult<TenantRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.TenantRole.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantRolesQuery query)
    {
        var result = await _mediator.Send(query);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
