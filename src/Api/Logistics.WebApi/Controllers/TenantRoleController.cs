namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class TenantRoleController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public TenantRoleController(
        IMapper mapper,
        IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedDataResult<TenantRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetList([FromQuery] GetTenantRolesQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
