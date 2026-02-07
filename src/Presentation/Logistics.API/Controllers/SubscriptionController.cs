using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CancelSubscriptionCommand = Logistics.Application.Commands.CancelSubscriptionCommand;
using CreateSubscriptionCommand = Logistics.Application.Commands.CreateSubscriptionCommand;
using CreateSubscriptionPlanCommand = Logistics.Application.Commands.CreateSubscriptionPlanCommand;
using UpdateSubscriptionPlanCommand = Logistics.Application.Commands.UpdateSubscriptionPlanCommand;
using ChangeSubscriptionPlanCommand = Logistics.Application.Commands.ChangeSubscriptionPlanCommand;

namespace Logistics.API.Controllers;

[ApiController]
[Route("subscriptions")]
[Produces("application/json")]
public class SubscriptionController(IMediator mediator) : ControllerBase
{
    #region Subscriptions

    [HttpGet("{id:guid}", Name = "GetSubscriptionById")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionById(Guid id)
    {
        var result = await mediator.Send(new GetSubscriptionQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetSubscriptions")]
    [ProducesResponseType(typeof(PagedResponse<SubscriptionDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<SubscriptionDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost(Name = "CreateSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}/cancel", Name = "CancelSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] CancelSubscriptionCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}/change-plan", Name = "ChangeSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeSubscriptionPlan(Guid id, [FromBody] ChangeSubscriptionPlanCommand request)
    {
        request.SubscriptionId = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}/renew", Name = "RenewSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenewSubscription(Guid id, [FromBody] RenewSubscriptionCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> DeleteSubscription(Guid id)
    {
        var result = await mediator.Send(new DeleteSubscriptionCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Subscription Plans

    [HttpGet("plans/{id:guid}", Name = "GetSubscriptionPlanById")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    //[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionPlanById(Guid id)
    {
        var result = await mediator.Send(new GetSubscriptionPlanQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet("plans", Name = "GetSubscriptionPlans")]
    [ProducesResponseType(typeof(PagedResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    //[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<SubscriptionPlanDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost("plans", Name = "CreateSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("plans/{id:guid}", Name = "UpdateSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("plans/{id:guid}", Name = "DeleteSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{AppRoles.Manager}")]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id)
    {
        var result = await mediator.Send(new DeleteSubscriptionPlanCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion
}
