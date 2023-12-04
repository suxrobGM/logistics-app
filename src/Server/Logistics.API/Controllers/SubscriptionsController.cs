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
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseResult<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsQuery request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("plans/{id}")]
    [ProducesResponseType(typeof(ResponseResult<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionPlanById(string id)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanQuery {Id = id});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(PagedResponseResult<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost()]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSubscription(string id, [FromBody] UpdateSubscriptionCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSubscription(string id)
    {
        var result = await _mediator.Send(new DeleteSubscriptionCommand {Id = id});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("plans")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand request)
    {
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("plans/{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSubscriptionPlan(string id, [FromBody] UpdateSubscriptionPlanCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("plans/{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteSubscriptionPlan(string id)
    {
        var result = await _mediator.Send(new DeleteSubscriptionPlanCommand {Id = id});
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
