namespace Logistics.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TruckController : ControllerBase
{
    private readonly IMediator mediator;

    public TruckController(
        IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10)
    {
        var result = await mediator.Send(new GetTrucksQuery
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
    public async Task<IActionResult> Create([FromBody] CreateTruckCommand request)
    {
        var result = await mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
