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

    [HttpPost]
    //[RequiredScope("admin.write")]
    public async Task<IActionResult> Create([FromBody] CreateCargoCommand request)
    {
        var result = await mediator.Send(request);

        if (result.Success)
            return Ok(result);

        return BadRequest(result.Error);
    }
}
