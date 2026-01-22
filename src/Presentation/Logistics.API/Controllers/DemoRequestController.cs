using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

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
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
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
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateDemoRequest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateDemoRequest(Guid id, [FromBody] UpdateDemoRequestCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteDemoRequest")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteDemoRequest(Guid id)
    {
        var result = await mediator.Send(new DeleteDemoRequestCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
