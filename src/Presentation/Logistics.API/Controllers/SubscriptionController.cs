﻿using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Consts.Policies;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("subscriptions")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Subscriptions

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionById(string id)
    {
        var result = await _mediator.Send(new GetSubscriptionQuery {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelSubscription(string id, [FromBody] CancelSubscriptionCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("{id}/renew")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenewSubscription(string id, [FromBody] RenewSubscriptionCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> DeleteSubscription(string id)
    {
        var result = await _mediator.Send(new DeleteSubscriptionCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Subscription Plans
    
    [HttpGet("plans/{id}")]
    [ProducesResponseType(typeof(Result<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    //[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionPlanById(string id)
    {
        var result = await _mediator.Send(new GetSubscriptionPlanQuery {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(PagedResult<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    //[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("plans")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("plans/{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> UpdateSubscriptionPlan(string id, [FromBody] UpdateSubscriptionPlanCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("plans/{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> DeleteSubscriptionPlan(string id)
    {
        var result = await _mediator.Send(new DeleteSubscriptionPlanCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
