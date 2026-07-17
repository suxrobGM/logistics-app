using Logistics.Application.Modules.Integrations.FuelCards.Commands;
using Logistics.Application.Modules.Integrations.FuelCards.Queries;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("fuelcards")]
[Produces("application/json")]
public class FuelCardsController(IMediator mediator) : ControllerBase
{
    #region Provider Configuration

    [HttpGet("providers", Name = "GetFuelCardProviders")]
    [ProducesResponseType(typeof(List<FuelCardProviderConfigurationDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.FuelCard.View)]
    public async Task<IActionResult> GetProviders()
    {
        var result = await mediator.Send(new GetFuelCardProviderConfigurationsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("providers", Name = "CreateFuelCardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.FuelCard.Manage)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateFuelCardProviderConfigurationCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpDelete("providers/{providerId:guid}", Name = "DeleteFuelCardProvider")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.FuelCard.Manage)]
    public async Task<IActionResult> DeleteProvider(Guid providerId)
    {
        var result = await mediator.Send(new DeleteFuelCardProviderConfigurationCommand { Id = providerId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("sync", Name = "SyncFuelCardTransactions")]
    [ProducesResponseType(typeof(FuelCardSyncResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.FuelCard.Manage)]
    public async Task<IActionResult> Sync([FromBody] SyncFuelCardTransactionsCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion

    #region Transactions

    [HttpGet("transactions", Name = "GetFuelCardTransactions")]
    [ProducesResponseType(typeof(PagedResult<FuelCardTransactionDto>), StatusCodes.Status200OK)]
    [Authorize(Policy = Permission.FuelCard.View)]
    public async Task<IActionResult> GetTransactions([FromQuery] GetFuelCardTransactionsQuery query)
    {
        var result = await mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("transactions/{transactionId:guid}/assign", Name = "AssignFuelCardTransactionTruck")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.FuelCard.Manage)]
    public async Task<IActionResult> AssignTransaction(
        Guid transactionId,
        [FromBody] AssignFuelCardTransactionTruckCommand request)
    {
        request.TransactionId = transactionId;
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    [HttpPost("transactions/{transactionId:guid}/ignore", Name = "IgnoreFuelCardTransaction")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.FuelCard.Manage)]
    public async Task<IActionResult> IgnoreTransaction(Guid transactionId)
    {
        var result = await mediator.Send(new IgnoreFuelCardTransactionCommand { TransactionId = transactionId });
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }

    #endregion
}
