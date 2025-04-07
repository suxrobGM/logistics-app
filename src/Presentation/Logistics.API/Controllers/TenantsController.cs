using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using Logistics.Shared.Consts.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Tenants

    [HttpGet("{identifier}")]
    [ProducesResponseType(typeof(Result<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetTenantById(string identifier)
    {
        var includeConnectionString = HttpContext.User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin);
        var result = await _mediator.Send(new GetTenantQuery
        {
            Id = identifier,
            Name = identifier,
            IncludeConnectionString = includeConnectionString
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LoadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.View)]
    public async Task<IActionResult> GetTenantList([FromQuery] GetTenantsQuery query)
    {
        if (User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin))
        {
            query.IncludeConnectionStrings = true;
        }

        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Create)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Edit)]
    public async Task<IActionResult> UpdateTenant(string id, [FromBody] UpdateTenantCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Delete)]
    public async Task<IActionResult> DeleteTenant(string id)
    {
        var result = await _mediator.Send(new DeleteTenantCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
    
    
    #region Payment Methods
    
    [HttpGet("{tenantId}/payment-methods/{id}")]
    [ProducesResponseType(typeof(Result<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetTenantPaymentMethodById(string tenantId, string id)
    {
        var result = await _mediator.Send(new GetPaymentMethodQuery {Id = id, TenantId = tenantId});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("{tenantId}/payment-methods")]
    [ProducesResponseType(typeof(PagedResult<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetTenantPaymentMethods(string tenantId, [FromQuery] GetPaymentMethodsQuery query)
    {
        query.TenantId = tenantId;
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{tenantId}/payment-methods")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> CreateTenantPaymentMethod(string tenantId, [FromBody] CreatePaymentMethodCommand request)
    {
        request.TenantId = tenantId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("{tenantId}/payment-methods")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> UpdateTenantPaymentMethod(string tenantId, [FromBody] UpdatePaymentMethodCommand request)
    {
        request.TenantId = tenantId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("{tenantId}/payment-methods/default")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> SetDefaultTenantPaymentMethod(string tenantId, [FromBody] SetDefaultPaymentMethodCommand request)
    {
        request.TenantId = tenantId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{tenantId}/payment-methods/{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> DeleteTenantPaymentMethod(string tenantId, string id)
    {
        var result = await _mediator.Send(new DeletePaymentMethodCommand {Id = id, TenantId = tenantId});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    #endregion
}
