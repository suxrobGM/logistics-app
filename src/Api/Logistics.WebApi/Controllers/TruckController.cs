namespace Logistics.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TruckController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly IMediator mediator;

    public TruckController(
        IMapper mapper,
        IMediator mediator)
    {
        this.mapper = mapper;
        this.mediator = mediator;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DataResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetTruckByIdQuery
        {
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(PagedDataResult<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetList(string searchInput = "", int page = 1, int pageSize = 10)
    {
        var result = await mediator.Send(new GetTrucksQuery
        {
            SearchInput = searchInput,
            Page = page,
            PageSize = pageSize
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Create([FromBody] TruckDto request)
    {
        var result = await mediator.Send(mapper.Map<CreateTruckCommand>(request));

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Update(string id, [FromBody] TruckDto request)
    {
        var updateRequest = mapper.Map<UpdateTruckCommand>(request);
        updateRequest.Id = id;
        var result = await mediator.Send(updateRequest);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DataResult), StatusCodes.Status400BadRequest)]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await mediator.Send(new DeleteTruckCommand
        {
            Id = id
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
