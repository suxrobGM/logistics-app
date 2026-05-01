using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("containers")]
[Produces("application/json")]
public class ContainerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetContainerById")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Container.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetContainerByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(ErrorResponse.FromResult(result));
    }

    [HttpGet(Name = "GetContainers")]
    [ProducesResponseType(typeof(PagedResponse<ContainerDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.Container.View)]
    public async Task<IActionResult> GetList([FromQuery] GetContainersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(PagedResponse<ContainerDto>.FromPagedResult(result, query.Page, query.PageSize));
    }

    [HttpPost(Name = "CreateContainer")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Container.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateContainerCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}", Name = "UpdateContainer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Container.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContainerCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}/status", Name = "UpdateContainerStatus")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Container.Manage)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateContainerStatusCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPut("{id:guid}/terminal", Name = "SetContainerTerminal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Container.Manage)]
    public async Task<IActionResult> SetTerminal(Guid id, [FromBody] MoveContainerToTerminalCommand request)
    {
        request.Id = id;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "DeleteContainer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.Container.Manage)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteContainerCommand { Id = id });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
