using Logistics.Application.Modules.Operations.Trailers.Commands;
using Logistics.Application.Modules.Operations.Trailers.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("trailers")]
[Produces("application/json")]
public class TrailerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetTrailerById")]
    [ProducesResponseType(typeof(TrailerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Trailer.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetTrailerByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTrailers")]
    [ProducesResponseType(typeof(PagedResponse<TrailerDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Trailer.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTrailersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<TrailerDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateTrailer")]
    [ProducesResponseType(typeof(TrailerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Trailer.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateTrailerCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTrailer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Trailer.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrailerCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTrailer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Trailer.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTrailerCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
