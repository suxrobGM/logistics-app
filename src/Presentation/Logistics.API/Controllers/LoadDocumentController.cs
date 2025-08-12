using System.Security.Claims;

using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("loads/{loadId:guid}/documents")]
[ApiController]
public class LoadDocumentController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoadDocumentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<LoadDocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetLoadDocuments(Guid loadId, [FromQuery] GetLoadDocumentsQuery query)
    {
        query.LoadId = loadId;
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(Result<LoadDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetDocumentById(Guid loadId, Guid documentId)
    {
        var result = await _mediator.Send(new GetLoadDocumentByIdQuery { DocumentId = documentId });

        // Verify document belongs to the specified load
        if (result.Success && result.Data?.LoadId != loadId)
        {
            return BadRequest(Result.Fail("Document does not belong to the specified load"));
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Edit)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<IActionResult> UploadDocument(Guid loadId, [FromForm] UploadDocumentRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(Result.Fail("No file provided"));
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(Result.Fail("User not authenticated"));
        }

        var command = new UploadLoadDocumentCommand
        {
            LoadId = loadId,
            FileContent = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            Type = request.Type,
            Description = request.Description,
            UploadedById = userId
        };

        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> DownloadDocument(Guid loadId, Guid documentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(Result.Fail("User not authenticated"));
        }

        var result = await _mediator.Send(new DownloadLoadDocumentQuery
        {
            DocumentId = documentId,
            RequestedById = userId
        });

        if (!result.Success || result.Data == null)
        {
            return BadRequest(result);
        }

        return File(
            result.Data.FileContent,
            result.Data.ContentType,
            result.Data.OriginalFileName);
    }

    [HttpPut("{documentId:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Edit)]
    public async Task<IActionResult> UpdateDocument(Guid loadId, Guid documentId, [FromBody] UpdateLoadDocumentCommand request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(Result.Fail("User not authenticated"));
        }

        request.DocumentId = documentId;
        request.UpdatedById = userId;

        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Delete)]
    public async Task<IActionResult> DeleteDocument(Guid loadId, Guid documentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(Result.Fail("User not authenticated"));
        }

        var result = await _mediator.Send(new DeleteLoadDocumentCommand
        {
            DocumentId = documentId,
            RequestedById = userId
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class UploadDocumentRequest
{
    public required IFormFile File { get; set; }
    public required DocumentType Type { get; set; }
    public string? Description { get; set; }
}
