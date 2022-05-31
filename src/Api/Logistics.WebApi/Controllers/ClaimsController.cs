namespace Logistics.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post(AzureConnectorRequest request)
    {
        var result = await _mediator.Send(new AddUserRoleClaimsCommand
        {
            ConnectorRequest = request,
            AuthorizationHeader = Request.Headers.Authorization
        });

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}
