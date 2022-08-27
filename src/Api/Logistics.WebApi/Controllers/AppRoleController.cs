namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class AppRoleController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public AppRoleController(
        IMapper mapper,
        IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedDataResult<AppRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.Employee.CanRead)]
    public async Task<IActionResult> GetList([FromQuery] GetAppRolesQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
