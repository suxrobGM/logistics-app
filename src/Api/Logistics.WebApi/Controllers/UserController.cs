namespace Logistics.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly IMediator mediator;

    public UserController(
        IMapper mapper,
        IMediator mediator)
    {
        this.mapper = mapper;
        this.mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetUserByIdQuery
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
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10)
    {
        var result = await mediator.Send(new GetUsersQuery 
        {
            Page = page,
            PageSize = pageSize
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("get-by-extid/{externalId}")]
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var result = await mediator.Send(new UserExistsQuery
        {
            ExternalId = externalId
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Create([FromBody] UserDto user)
    {
        var result = await mediator.Send(mapper.Map<CreateUserCommand>(user));

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Update(string id, [FromBody] UserDto request)
    {
        var updateRequest = mapper.Map<UpdateUserCommand>(request);
        updateRequest.Id = id;
        var result = await mediator.Send(updateRequest);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
