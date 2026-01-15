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
[Produces("application/json")]
public class DocumentController(IMediator mediator) : ControllerBase
{
    private const int MaxUploadSizeBytes = 20 * 1024 * 1024; // 20 MB

    // GET /documents/{owner}/{ownerId}
    [HttpGet(Name = "GetDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetDocuments([FromQuery] GetDocumentsQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /documents/{documentId}
    [HttpGet("{documentId:guid}", Name = "GetDocumentById")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetDocumentById(Guid documentId)
    {
        var result = await mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound(ErrorResponse.FromResult(result));
        }

        return Ok(result.Value);
    }

    // POST /documents/upload
    [HttpPost("upload", Name = "UploadDocument")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(MaxUploadSizeBytes)]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(new ErrorResponse("No file provided"));
        }

        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
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
            Description = request.Description,
            UploadedById = userId.Value
        };

        var result = await mediator.Send(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /documents/{documentId}/download
    [HttpGet("{documentId:guid}/download", Name = "DownloadDocument")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> DownloadDocument(Guid documentId)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        var result = await mediator.Send(new DownloadDocumentQuery
        {
            DocumentId = documentId,
            RequestedById = userId.Value
        });

        if (!result.IsSuccess || result.Value is null)
        {
            return NotFound(ErrorResponse.FromResult(result));
        }

        return File(result.Value.FileContent, result.Value.ContentType, result.Value.OriginalFileName);
    }

    // PUT /documents/{documentId}
    [HttpPut("{documentId:guid}", Name = "UpdateDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> UpdateDocument(Guid documentId, [FromBody] UpdateDocumentCommand request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        request.DocumentId = documentId;
        request.UpdatedById = userId.Value;

        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    // DELETE /documents/{documentId}
    [HttpDelete("{documentId:guid}", Name = "DeleteDocument")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var result = await mediator.Send(new DeleteDocumentCommand
        {
            DocumentId = documentId
        });

        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    // POST /documents/pod
    [HttpPost("pod", Name = "CaptureProofOfDelivery")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(MaxUploadSizeBytes)] // 20MB limit for multiple photos
    public async Task<IActionResult> CaptureProofOfDelivery([FromForm] CaptureProofOfDeliveryRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        var photos = new List<FileUpload>();
        if (request.Photos != null)
        {
            foreach (var photo in request.Photos)
            {
                if (photo.Length > 0)
                {
                    photos.Add(new FileUpload
                    {
                        Content = photo.OpenReadStream(),
                        FileName = photo.FileName,
                        ContentType = photo.ContentType,
                        FileSizeBytes = photo.Length
                    });
                }
            }
        }

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
            CapturedById = userId.Value
        };

        var result = await mediator.Send(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    // POST /documents/bol
    [HttpPost("bol", Name = "CaptureBillOfLading")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(MaxUploadSizeBytes)] // 20MB limit for multiple photos
    public async Task<IActionResult> CaptureBillOfLading([FromForm] CaptureProofOfDeliveryRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        var photos = new List<FileUpload>();
        if (request.Photos != null)
        {
            foreach (var photo in request.Photos)
            {
                if (photo.Length > 0)
                {
                    photos.Add(new FileUpload
                    {
                        Content = photo.OpenReadStream(),
                        FileName = photo.FileName,
                        ContentType = photo.ContentType,
                        FileSizeBytes = photo.Length
                    });
                }
            }
        }

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
            CapturedById = userId.Value
        };

        var result = await mediator.Send(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
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
    string? Notes);
