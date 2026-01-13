using Logistics.API.Extensions;
using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateLoadCommand = Logistics.Application.Commands.CreateLoadCommand;
using GetLoadsQuery = Logistics.Application.Queries.GetLoadsQuery;
using UpdateLoadCommand = Logistics.Application.Commands.UpdateLoadCommand;

namespace Logistics.API.Controllers;

[ApiController]
[Route("loads")]
public class LoadController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetLoadById")]
    [ProducesResponseType(typeof(LoadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetLoadByIdQuery { Id = id });
        return result.Success ? Ok(result.Data) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetLoads")]
    [ProducesResponseType(typeof(PagedResponse<LoadDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Loads.View)]
    public async Task<IActionResult> GetList([FromQuery] GetLoadsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<LoadDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateLoad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Create)]
    public async Task<IActionResult> Create([FromBody] CreateLoadCommand request)
    {
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateLoad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLoadCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.Success ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteLoad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permissions.Loads.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteLoadCommand { Id = id });
        return result.Success ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpPost("import", Name = "ImportLoadFromPdf")]
    [ProducesResponseType(typeof(ImportLoadFromPdfResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Loads.Create)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<IActionResult> ImportFromPdf([FromForm] ImportLoadFromPdfRequest request)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(new ErrorResponse("No PDF file provided"));
        }

        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("User not authenticated"));
        }

        var cmd = new ImportLoadFromPdfCommand
        {
            PdfContent = request.File.OpenReadStream(),
            FileName = request.File.FileName,
            CurrentUserId = userId,
            AssignedTruckId = request.AssignedTruckId
        };

        var result = await mediator.Send(cmd);
        return result.Success ? Ok(result.Data) : BadRequest(ErrorResponse.FromResult(result));
    }
}

/// <summary>
///     Request model for importing a load from PDF.
/// </summary>
public record ImportLoadFromPdfRequest(
    IFormFile File,
    Guid AssignedTruckId);
