using Logistics.API.Extensions;
using Logistics.Application.Modules.Operations.Documents.Commands;
using Logistics.Application.Modules.Operations.Documents.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("documents")]
[Produces("application/json")]
public class DocumentController(IMediator mediator) : ControllerBase
{
    private const int MaxUploadSizeBytes = 20 * 1024 * 1024; // 20 MB

    // GET /documents/{owner}/{ownerId}
    [HttpGet(Name = "GetDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Document.View)]
    public async Task<IActionResult> GetDocuments([FromQuery] GetDocumentsQuery query)
    {
        var result = await mediator.Send(query);
        return result.ToActionResult();
    }

    // GET /documents/{documentId}
    [HttpGet("{documentId:guid}", Name = "GetDocumentById")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Document.View)]
    public async Task<IActionResult> GetDocumentById(Guid documentId)
    {
        var result = await mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
        return result.ToActionResult();
    }

    // POST /documents/upload
    [HttpPost("upload", Name = "UploadDocument")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Document.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(new ErrorResponse("No file provided"));
        }

        var cmd = new UploadDocumentCommand
        {
            OwnerType = request.OwnerType,
            OwnerId = request.OwnerId,
            FileContent = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            Type = request.Type,
            Description = request.Description
        };

        var result = await mediator.Send(cmd);
        return result.ToActionResult();
    }

    // GET /documents/{documentId}/download
    [HttpGet("{documentId:guid}/download", Name = "DownloadDocument")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Document.View)]
    public async Task<IActionResult> DownloadDocument(Guid documentId)
    {
        var result = await mediator.Send(new DownloadDocumentQuery { DocumentId = documentId });

        return result.ToFileResult(x => File(x.FileContent, x.ContentType, x.OriginalFileName));
    }

    // PUT /documents/{documentId}
    [HttpPut("{documentId:guid}", Name = "UpdateDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Document.Manage)]
    public async Task<IActionResult> UpdateDocument(Guid documentId, [FromBody] UpdateDocumentCommand request)
    {
        request.DocumentId = documentId;

        var result = await mediator.Send(request);
        return result.ToActionResult();
    }

    // DELETE /documents/{documentId}
    [HttpDelete("{documentId:guid}", Name = "DeleteDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Document.Manage)]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var result = await mediator.Send(new DeleteDocumentCommand
        {
            DocumentId = documentId
        });

        return result.ToActionResult();
    }

    // POST /documents/pod
    [HttpPost("pod", Name = "CaptureProofOfDelivery")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Document.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadSizeBytes)] // 20MB limit for multiple photos
    public async Task<IActionResult> CaptureProofOfDelivery([FromForm] CaptureProofOfDeliveryRequest request)
    {
        var photos = request.Photos.ToFileUploads();

        var cmd = new CaptureProofOfDeliveryCommand
        {
            LoadId = request.LoadId,
            TripStopId = request.TripStopId,
            Photos = photos,
            SignatureBase64 = request.SignatureBase64,
            RecipientName = request.RecipientName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes,
            UnableToSignReason = request.UnableToSignReason
        };

        var result = await mediator.Send(cmd);
        return result.ToActionResult();
    }

    // POST /documents/bol
    [HttpPost("bol", Name = "CaptureBillOfLading")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Document.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadSizeBytes)] // 20MB limit for multiple photos
    public async Task<IActionResult> CaptureBillOfLading([FromForm] CaptureProofOfDeliveryRequest request)
    {
        var photos = request.Photos.ToFileUploads();

        var cmd = new CaptureBillOfLadingCommand
        {
            LoadId = request.LoadId,
            TripStopId = request.TripStopId,
            Photos = photos,
            SignatureBase64 = request.SignatureBase64,
            RecipientName = request.RecipientName,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes,
            UnableToSignReason = request.UnableToSignReason
        };

        var result = await mediator.Send(cmd);
        return result.ToActionResult();
    }
}

public record UploadDocumentRequest(
    DocumentOwnerType OwnerType,
    Guid OwnerId,
    IFormFile File,
    DocumentType Type,
    string? Description);

public record CaptureProofOfDeliveryRequest(
    Guid LoadId,
    Guid? TripStopId,
    List<IFormFile>? Photos,
    string? SignatureBase64,
    string? RecipientName,
    double? Latitude,
    double? Longitude,
    string? Notes,
    string? UnableToSignReason);
