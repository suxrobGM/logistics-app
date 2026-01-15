using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Inspection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("inspections")]
[Produces("application/json")]
public class InspectionController(IMediator mediator) : ControllerBase
{
    // POST /inspections/decode-vin
    [HttpPost("decode-vin", Name = "DecodeVin")]
    [ProducesResponseType(typeof(VehicleInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> DecodeVin([FromBody] DecodeVinRequest request)
    {
        var result = await mediator.Send(new DecodeVinCommand { Vin = request.Vin });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /inspections/condition-reports
    [HttpGet("condition-reports", Name = "GetConditionReports")]
    [ProducesResponseType(typeof(List<ConditionReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetConditionReports([FromQuery] Guid? loadId)
    {
        var result = await mediator.Send(new GetConditionReportsQuery { LoadId = loadId });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    // GET /inspections/condition-reports/{id}
    [HttpGet("condition-reports/{id:guid}", Name = "GetConditionReportById")]
    [ProducesResponseType(typeof(ConditionReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> GetConditionReportById(Guid id)
    {
        var result = await mediator.Send(new GetConditionReportsQuery { VehicleConditionReportId = id });

        if (!result.IsSuccess || result.Value is null || result.Value.Count == 0)
        {
            return NotFound(new ErrorResponse($"Condition report with ID '{id}' not found"));
        }

        return Ok(result.Value.First());
    }

    // POST /inspections/condition-reports
    [HttpPost("condition-reports", Name = "CreateConditionReport")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> CreateConditionReport([FromForm] CreateConditionReportRequest request)
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

        var cmd = new CreateConditionReportCommand
        {
            LoadId = request.LoadId,
            Vin = request.Vin,
            Type = request.Type,
            VehicleYear = request.VehicleYear,
            VehicleMake = request.VehicleMake,
            VehicleModel = request.VehicleModel,
            VehicleBodyClass = request.VehicleBodyClass,
            DamageMarkersJson = request.DamageMarkersJson,
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
/// Request to create a vehicle condition report with photos and damage markers.
/// </summary>
public record CreateConditionReportRequest(
    Guid LoadId,
    string Vin,
    InspectionType Type,
    int? VehicleYear,
    string? VehicleMake,
    string? VehicleModel,
    string? VehicleBodyClass,
    string? DamageMarkersJson,
    string? Notes,
    string? SignatureBase64,
    List<IFormFile>? Photos,
    double? Latitude,
    double? Longitude);
