using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("contact-submissions")]
[Produces("application/json")]
public class ContactController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetContactSubmissionById")]
    [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetContactSubmissionById(Guid id)
    {
        var result = await mediator.Send(new GetContactSubmissionQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetContactSubmissions")]
    [ProducesResponseType(typeof(PagedResponse<ContactSubmissionDto>), StatusCodes.Status200OK)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> GetContactSubmissions([FromQuery] GetContactSubmissionsQuery request)
    {
        var result = await mediator.Send(request);
        return Ok(PagedResponse<ContactSubmissionDto>.FromPagedResult(result, request.Page, request.PageSize));
    }

    [HttpPost(Name = "CreateContactSubmission")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> CreateContactSubmission([FromBody] CreateContactSubmissionCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateContactSubmission")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateContactSubmission(Guid id, [FromBody] UpdateContactSubmissionCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteContactSubmission")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteContactSubmission(Guid id)
    {
        var result = await mediator.Send(new DeleteContactSubmissionCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
