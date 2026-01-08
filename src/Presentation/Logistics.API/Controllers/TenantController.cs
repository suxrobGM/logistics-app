using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CreateTenantCommand = Logistics.Application.Commands.CreateTenantCommand;
using UpdateTenantCommand = Logistics.Application.Commands.UpdateTenantCommand;

namespace Logistics.API.Controllers;

[ApiController]
[Route("tenants")]
public class TenantController(IMediator mediator) : ControllerBase
{
    #region Tenants

    [HttpGet("{identifier}", Name = "GetTenantById")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetTenantById(string identifier)
    {
        var includeConnectionString = HttpContext.User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin);
        var result = await mediator.Send(new GetTenantQuery
        {
            Id = Guid.TryParse(identifier, out var id) ? id : null,
            Name = identifier,
            IncludeConnectionString = includeConnectionString
        });

        return result.Success ? Ok(result.Data) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTenants")]
    [ProducesResponseType(typeof(PagedResponse<TenantDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Tenants.View)]
    public async Task<IActionResult> GetTenantList([FromQuery] GetTenantsQuery query)
    {
        if (User.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin)) query.IncludeConnectionStrings = true;

        var result = await mediator.Send(query);
        return Ok(PagedResponse<TenantDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateTenant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Create)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTenant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Tenants.Edit)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTenant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Tenants.Delete)]
    public async Task<IActionResult> DeleteTenant(Guid id)
    {
        var result = await mediator.Send(new DeleteTenantCommand { Id = id });
        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    #endregion
}
