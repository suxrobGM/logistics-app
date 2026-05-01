using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("terminals")]
[Produces("application/json")]
public class TerminalController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetTerminalById")]
    [ProducesResponseType(typeof(TerminalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Terminal.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetTerminalByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetTerminals")]
    [ProducesResponseType(typeof(PagedResponse<TerminalDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Terminal.View)]
    public async Task<IActionResult> GetList([FromQuery] GetTerminalsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<TerminalDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateTerminal")]
    [ProducesResponseType(typeof(TerminalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Terminal.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateTerminalCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateTerminal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Terminal.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTerminalCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteTerminal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Terminal.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteTerminalCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
