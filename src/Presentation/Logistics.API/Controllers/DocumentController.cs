using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController(IMediator mediator) : ControllerBase
{
    // GET /documents/{owner}/{ownerId}
    [HttpGet(Name = "GetDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetDocuments([FromQuery] GetDocumentsQuery query)
    {
        var result = await mediator.Send(query);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /documents/{documentId}
    [HttpGet("{documentId:guid}", Name = "GetDocumentById")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetDocumentById(Guid documentId)
    {
        var result = await mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
        if (!result.Success || result.Data is null) return NotFound(ErrorResponse.FromResult(result));
        return Ok(result.Data);
    }

    // POST /documents/upload
    [HttpPost("upload", Name = "UploadDocument")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        if (request.File.Length == 0)
            return BadRequest(new ErrorResponse("No file provided"));

        var userId = User.GetUserId();
        if (userId == Guid.Empty) return BadRequest(new ErrorResponse("User not authenticated"));

        var cmd = new UploadDocumentCommand
        {
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            FileContent = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            Type = request.Type,
            Description = request.Description,
            UploadedById = userId
        };

        var result = await mediator.Send(cmd);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /documents/{documentId}/download
    [HttpGet("{documentId:guid}/download", Name = "DownloadDocument")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> DownloadDocument(Guid documentId)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty) return BadRequest(new ErrorResponse("User not authenticated"));

        var result = await mediator.Send(new DownloadDocumentQuery
        {
            DocumentId = documentId,
            RequestedById = userId
        });

        if (!result.Success || result.Data is null) return NotFound(ErrorResponse.FromResult(result));

        return File(result.Data.FileContent, result.Data.ContentType, result.Data.OriginalFileName);
    }

    // PUT /documents/{documentId}
    [HttpPut("{documentId:guid}", Name = "UpdateDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> UpdateDocument(Guid documentId, [FromBody] UpdateDocumentCommand request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty) return BadRequest(new ErrorResponse("User not authenticated"));

        request.DocumentId = documentId;
        request.UpdatedById = userId;

        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    // DELETE /documents/{documentId}
    [HttpDelete("{documentId:guid}", Name = "DeleteDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty) return BadRequest(new ErrorResponse("User not authenticated"));

        var result = await mediator.Send(new DeleteDocumentCommand
        {
            DocumentId = documentId
        });

        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    private static DocumentOwnerType ParseOwner(string owner)
    {
        return owner.Equals("loads", StringComparison.OrdinalIgnoreCase)
            ? DocumentOwnerType.Load
            : DocumentOwnerType.Employee;
    }
}

public record UploadDocumentRequest(
    DocumentOwnerType OwnerType,
    Guid OwnerId,
    IFormFile File,
    DocumentType Type,
    string? Description);
