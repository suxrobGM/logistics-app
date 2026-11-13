using Logistics.API.Extensions;
using Logistics.Application.Modules.Tenancy.Subscriptions.Commands;
using Logistics.Application.Modules.Tenancy.Subscriptions.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetSubscriptionById(Guid id)
    {
        var result = await mediator.Send(new GetSubscriptionQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet(Name = "GetSubscriptions")]
    [ProducesResponseType(typeof(PagedResponse<SubscriptionDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<SubscriptionDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost(Name = "CreateSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/cancel", Name = "CancelSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{TenantRoles.Owner},{TenantRoles.Manager}")]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] CancelSubscriptionCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/change-plan", Name = "ChangeSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{TenantRoles.Owner},{TenantRoles.Manager}")]
    public async Task<IActionResult> ChangeSubscriptionPlan(Guid id, [FromBody] ChangeSubscriptionPlanCommand request)
    {
        request.SubscriptionId = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/renew", Name = "RenewSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{TenantRoles.Owner},{TenantRoles.Manager}")]
    public async Task<IActionResult> RenewSubscription(Guid id, [FromBody] RenewSubscriptionCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteSubscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteSubscription(Guid id)
    {
        var result = await mediator.Send(new DeleteSubscriptionCommand { Id = id });
        return result.ToActionResult();
    }

    [HttpGet("billing-portal", Name = "GetBillingPortalUrl")]
    [ProducesResponseType(typeof(BillingPortalUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin},{TenantRoles.Owner},{TenantRoles.Manager}")]
    public async Task<IActionResult> GetBillingPortalUrl([FromQuery] string returnUrl)
    {
        var result = await mediator.Send(new GetBillingPortalUrlQuery { ReturnUrl = returnUrl });
        return result.ToActionResult();
    }

    #endregion

    #region Subscription Plans

    [HttpGet("plans/{id:guid}", Name = "GetSubscriptionPlanById")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetSubscriptionPlanById(Guid id)
    {
        var result = await mediator.Send(new GetSubscriptionPlanQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet("plans", Name = "GetSubscriptionPlans")]
    [ProducesResponseType(typeof(PagedResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [Authorize]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] GetSubscriptionPlansQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<SubscriptionPlanDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost("plans", Name = "CreateSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("plans/{id:guid}", Name = "UpdateSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpdateSubscriptionPlanCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("plans/{id:guid}", Name = "DeleteSubscriptionPlan")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id)
    {
        var result = await mediator.Send(new DeleteSubscriptionPlanCommand { Id = id });
        return result.ToActionResult();
    }

    #endregion
}
