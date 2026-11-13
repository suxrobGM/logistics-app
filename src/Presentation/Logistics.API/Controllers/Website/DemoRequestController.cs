using Logistics.API.Extensions;
using Logistics.Application.Modules.Website.DemoRequests.Commands;
using Logistics.Application.Modules.Website.DemoRequests.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.Website;

[ApiController]
[Route("demo-requests")]
[Produces("application/json")]
public class DemoRequestController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetDemoRequestById")]
    [ProducesResponseType(typeof(DemoRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetDemoRequestById(Guid id)
    {
        var result = await mediator.Send(new GetDemoRequestQuery { Id = id });
        return result.ToActionResult();
    }

    [HttpGet(Name = "GetDemoRequests")]
    [ProducesResponseType(typeof(PagedResponse<DemoRequestDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetDemoRequests([FromQuery] GetDemoRequestsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<DemoRequestDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost(Name = "CreateDemoRequest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> CreateDemoRequest([FromBody] CreateDemoRequestCommand request)
    {
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}", Name = "UpdateDemoRequest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateDemoRequest(Guid id, [FromBody] UpdateDemoRequestCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}", Name = "DeleteDemoRequest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteDemoRequest(Guid id)
    {
        var result = await mediator.Send(new DeleteDemoRequestCommand { Id = id });
        return result.ToActionResult();
    }
}
