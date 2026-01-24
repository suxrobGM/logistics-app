using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     Controller for managing public tracking links.
/// </summary>
[ApiController]
[Route("tracking")]
[Produces("application/json")]
public class TrackingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Get public tracking information for a load (anonymous access).
    /// </summary>
    [HttpGet("{tenantId:guid}/{token}", Name = "GetPublicTracking")]
    [ProducesResponseType(typeof(PublicTrackingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicTracking(Guid tenantId, string token)
    {
        var result = await mediator.Send(new GetPublicTrackingQuery
        {
            TenantId = tenantId,
            Token = token
        });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Get documents for a tracked load (anonymous access).
    /// </summary>
    [HttpGet("{tenantId:guid}/{token}/documents", Name = "GetPublicTrackingDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocuments(Guid tenantId, string token)
    {
        var result = await mediator.Send(new GetPublicTrackingDocumentsQuery
        {
            TenantId = tenantId,
            Token = token
        });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Download a document for a tracked load (anonymous access).
    /// </summary>
    [HttpGet("{tenantId:guid}/{token}/documents/{documentId:guid}/download", Name = "DownloadPublicTrackingDocument")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadDocument(Guid tenantId, string token, Guid documentId)
    {
        var result = await mediator.Send(new DownloadPublicTrackingDocumentQuery
        {
            TenantId = tenantId,
            Token = token,
            DocumentId = documentId
        });

        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound(ErrorResponse.FromResult(result));
        }

        return File(result.Value.FileContent, result.Value.ContentType, result.Value.OriginalFileName);
    }

    /// <summary>
    ///     Get tracking links for a specific load (staff only).
    /// </summary>
    [HttpGet("load/{loadId:guid}", Name = "GetTrackingLinksForLoad")]
    [ProducesResponseType(typeof(IEnumerable<TrackingLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> GetTrackingLinksForLoad(Guid loadId)
    {
        var result = await mediator.Send(new GetTrackingLinksForLoadQuery { LoadId = loadId });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Create a new tracking link for a load (staff only).
    /// </summary>
    [HttpPost(Name = "CreateTrackingLink")]
    [ProducesResponseType(typeof(TrackingLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> Create([FromBody] CreateTrackingLinkCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Revoke a tracking link (staff only).
    /// </summary>
    [HttpDelete("{id:guid}", Name = "RevokeTrackingLink")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Load.Manage)]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await mediator.Send(new RevokeTrackingLinkCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     Send a tracking link by email (staff only).
    /// </summary>
    [HttpPost("{id:guid}/email", Name = "SendTrackingLinkEmail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.View)]
    public async Task<IActionResult> SendEmail(Guid id, [FromBody] SendTrackingLinkEmailRequest request)
    {
        var result = await mediator.Send(new SendTrackingLinkEmailCommand
        {
            TrackingLinkId = id,
            RecipientEmail = request.Email,
            PersonalMessage = request.PersonalMessage
        });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}

public record SendTrackingLinkEmailRequest(string Email, string? PersonalMessage);
