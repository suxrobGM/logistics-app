using Logistics.API.Extensions;
using Logistics.Application.Modules.Compliance.Inspections.Commands;
using Logistics.Application.Modules.Compliance.Inspections.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("inspections")]
[Produces("application/json")]
public class InspectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetInspections")]
    [ProducesResponseType(typeof(List<ConditionReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetInspections([FromQuery] Guid? loadId)
    {
        var result = await mediator.Send(new GetConditionReportsQuery { LoadId = loadId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("parts", Name = "GetInspectionParts")]
    [ProducesResponseType(typeof(InspectionPartCatalogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetInspectionParts([FromQuery] LoadType loadType)
    {
        var result = await mediator.Send(new GetInspectionPartCatalogQuery { LoadType = loadType });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpGet("{id:guid}", Name = "GetInspection")]
    [ProducesResponseType(typeof(ConditionReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetInspection(Guid id)
    {
        var result = await mediator.Send(new GetConditionReportsQuery { ConditionReportId = id });

        if (!result.IsSuccess || result.Value is null || result.Value.Count == 0)
        {
            return NotFound(new ErrorResponse($"Inspection with ID '{id}' not found"));
        }

        return Ok(result.Value.First());
    }

    [HttpPost(Name = "CreateInspection")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> CreateInspection([FromForm] CreateInspectionRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        var photos = new List<FileUpload>();
        if (request.Photos is not null)
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

        // Defects arrive as a JSON-encoded string in the multipart form because
        // POST is multipart/form-data (Photos are files). Parse here.
        var defects = ConditionDefectInput.ParseDefects(request.DefectsJson);

        var cmd = new CreateConditionReportCommand
        {
            LoadId = request.LoadId,
            Type = request.Type,
            Vin = request.Vin,
            VehicleYear = request.VehicleYear,
            VehicleMake = request.VehicleMake,
            VehicleModel = request.VehicleModel,
            VehicleBodyClass = request.VehicleBodyClass,
            ContainerNumber = request.ContainerNumber,
            SealNumber = request.SealNumber,
            Defects = defects,
            Notes = request.Notes,
            SignatureBase64 = request.SignatureBase64,
            Photos = photos,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            InspectedById = userId.Value
        };

        var result = await mediator.Send(cmd);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }
}

/// <summary>
/// Multipart request to create a cargo inspection. Defects are submitted
/// as a JSON string so they can travel alongside Photos in the same form.
/// </summary>
public record CreateInspectionRequest(
    Guid LoadId,
    InspectionType Type,
    string? Vin,
    int? VehicleYear,
    string? VehicleMake,
    string? VehicleModel,
    string? VehicleBodyClass,
    string? ContainerNumber,
    string? SealNumber,
    string? DefectsJson,
    string? Notes,
    string? SignatureBase64,
    List<IFormFile>? Photos,
    double? Latitude,
    double? Longitude);
