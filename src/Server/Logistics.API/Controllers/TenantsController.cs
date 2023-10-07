using Logistics.Domain.ValueObjects;
using Logistics.Models;

namespace Logistics.API.Controllers;

[Route("tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{identifier}")]
    [ProducesResponseType(typeof(ResponseResult<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetById(string identifier)
    {
        var includeConnectionString = HttpContext.User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin);
        var result = await _mediator.Send(new GetTenantQuery
        {
            Id = identifier,
            Name = identifier,
            IncludeConnectionString = includeConnectionString
        });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<LoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantsQuery query)
    {
        if (User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin))
        {
            query.IncludeConnectionStrings = true;
        }

        var result = await _mediator.Send(query);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTenantCommand request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTenantCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteTenantCommand
        {
            Id = id
        });

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}
