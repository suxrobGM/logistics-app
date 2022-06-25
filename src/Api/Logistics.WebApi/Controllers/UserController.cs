namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UserController(
        IMapper mapper,
        IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.User.CanRead)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery
        {
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedDataResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.User.CanRead)]
    public async Task<IActionResult> GetList([FromQuery] GetUsersQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
