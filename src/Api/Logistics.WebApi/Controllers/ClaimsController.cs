namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public ClaimsController(
        IConfiguration configuration,
        IMediator mediator)
    {
        _configuration = configuration;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post(AzureConnectorRequest request)
    {
        var clientId = _configuration["AzureAd:ClientId"];
        var authHeader = Request.Headers.Authorization;
        var result = await _mediator.Send(new AddUserRoleClaimsCommand(request, authHeader, clientId));

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
