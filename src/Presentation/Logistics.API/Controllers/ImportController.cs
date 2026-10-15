using System.Text;
using System.Text.Json;
using Logistics.Application.Modules.Platform.DataImport.Commands;
using Logistics.Application.Modules.Platform.DataImport.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

/// <summary>
///     Bulk CSV/XLSX import. Stateless: the client re-sends the file with each step (preview, validate,
///     import), so nothing is stored between them and the upload never touches disk or blob storage.
/// </summary>
[ApiController]
[Route("imports")]
[Produces("application/json")]
[Authorize(Policy = Permission.Import.Manage)]
public class ImportController(IMediator mediator) : ControllerBase
{
    private const long RequestLimitBytes = 11 * 1024 * 1024;

    private static readonly JsonSerializerOptions MappingJson = new(JsonSerializerDefaults.Web);

    [HttpGet("templates/{entityType}", Name = "GetImportTemplate")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplate(ImportEntityType entityType)
    {
        var result = await mediator.Send(new GetImportTemplateQuery { EntityType = entityType });
        var fileName = $"{entityType.ToString().ToLowerInvariant()}-import-template.csv";
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(result.Value!)).ToArray(),
            "text/csv", fileName);
    }

    [HttpPost("preview", Name = "PreviewImport")]
    [ProducesResponseType(typeof(ImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(RequestLimitBytes)]
    public async Task<IActionResult> Preview([FromForm] PreviewImportRequest request)
    {
        await using var content = await BufferAsync(request.File);
        var result = await mediator.Send(new PreviewImportQuery
        {
            Content = content,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            EntityType = request.EntityType
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("validate", Name = "ValidateImport")]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(RequestLimitBytes)]
    public Task<IActionResult> Validate([FromForm] RunImportRequest request) => RunAsync(request, dryRun: true);

    [HttpPost(Name = "RunImport")]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(RequestLimitBytes)]
    public Task<IActionResult> Run([FromForm] RunImportRequest request) => RunAsync(request, dryRun: false);

    private async Task<IActionResult> RunAsync(RunImportRequest request, bool dryRun)
    {
        IReadOnlyList<ImportColumnMappingDto>? mapping;
        try
        {
            mapping = JsonSerializer.Deserialize<IReadOnlyList<ImportColumnMappingDto>>(request.Mapping, MappingJson);
        }
        catch (JsonException)
        {
            mapping = null;
        }

        if (mapping is null)
        {
            return BadRequest(new ErrorResponse("The column mapping is not valid JSON."));
        }

        await using var content = await BufferAsync(request.File);
        var result = await mediator.Send(new RunImportCommand
        {
            Content = content,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            EntityType = request.EntityType,
            Mapping = mapping,
            DryRun = dryRun,
            UpdateExisting = request.UpdateExisting,
            SendInvitations = request.SendInvitations
        });
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    /// <summary>
    ///     XLSX is a zip and needs a seekable stream; the size cap keeps this buffer small.
    /// </summary>
    private static async Task<MemoryStream> BufferAsync(IFormFile file)
    {
        var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        buffer.Position = 0;
        return buffer;
    }
}

public record PreviewImportRequest(IFormFile File, ImportEntityType EntityType);

/// <param name="Mapping">JSON array of <see cref="ImportColumnMappingDto"/>, as returned by preview and edited by the user.</param>
public record RunImportRequest(
    IFormFile File,
    ImportEntityType EntityType,
    string Mapping,
    bool UpdateExisting = false,
    bool SendInvitations = false);
