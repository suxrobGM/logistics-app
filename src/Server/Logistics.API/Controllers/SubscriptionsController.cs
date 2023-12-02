using Logistics.Shared.Models;
using Logistics.Shared.Roles;

namespace Logistics.API.Controllers;

[Route("subscriptions")]
[ApiController]
[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionById(string id)
    {
        var result = await _mediator.Send(new GetSubscriptionQuery {Id = id});

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("plans/{id}")]
    [ProducesResponseType(typeof(ResponseResult<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionPlanById(string id)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanQuery {Id = id});

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(PagedResponseResult<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery request)
    {
        var result = await _mediator.Send(request);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
