using Logistics.API.Extensions;
using Logistics.Application.Modules.AI.AISettings.Commands;
using Logistics.Application.Modules.AI.AISettings.Queries;
using Logistics.Mediator;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers.AI;

[ApiController]
[Route("ai/settings")]
[Produces("application/json")]
public class AISettingsController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetAISettings")]
    [ProducesResponseType(typeof(AISettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> GetAISettings()
    {
        var result = await mediator.Send(new GetAISettingsQuery());
        return result.ToActionResult();
    }

    [HttpPut(Name = "UpdateAISettings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Tenant.Manage)]
    public async Task<IActionResult> UpdateAISettings([FromBody] UpdateAISettingsCommand command)
    {
        var result = await mediator.Send(command);
        return result.ToActionResult();
    }
}
