namespace Logistics.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CargoController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly IMediator mediator;

    public CargoController(
        IMapper mapper,
        IMediator mediator)
    {
        this.mapper = mapper;
        this.mediator = mediator;
    }

    [HttpGet("list")]
    //[RequiredScope("admin.read")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10)
    {
        var result = await mediator.Send(new GetCargoesQuery
        {
            Page = page,
            PageSize = pageSize
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpPost("create")]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Create([FromBody] CargoDto request)
    {
        var result = await mediator.Send(mapper.Map<CreateCargoCommand>(request));

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }

    [HttpPut("update")]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Update(string id, [FromBody] CargoDto request)
    {
        var updateRequest = mapper.Map<UpdateCargoCommand>(request);
        updateRequest.Id = id;
        var result = await mediator.Send(updateRequest);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
