using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("apikeys")]
[Produces("application/json")]
public class ApiKeysController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetApiKeys")]
    [ProducesResponseType(typeof(List<ApiKeyDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.ApiKey.View)]
    public async Task<IActionResult> GetApiKeys()
    {
        var result = await mediator.Send(new GetApiKeysQuery());
        return Ok(result.Value);
    }

    [HttpPost(Name = "CreateApiKey")]
    [ProducesResponseType(typeof(ApiKeyCreatedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.ApiKey.Manage)]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("{id:guid}", Name = "RevokeApiKey")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = Permission.ApiKey.Manage)]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        var result = await mediator.Send(new RevokeApiKeyCommand { Id = id });
        return result.IsSuccess ? NoContent() : NotFound(ErrorResponse.FromResult(result));
    }
}
