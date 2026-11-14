using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class RevertAIDispatchDecisionHandler(
    ITenantUnitOfWork tenantUow,
    IMediator mediator,
    IAgentDecisionNotes notes,
    ICurrentUserService currentUser,
    IAIDispatchBroadcastService broadcastService) : IRequestHandler<RevertAIDispatchDecisionCommand, Result>
{
    public async Task<Result> Handle(RevertAIDispatchDecisionCommand request, CancellationToken ct)
    {
        var decision = await tenantUow.Repository<AgentDecision>().Query()
            .DispatchOnly()
            .FirstOrDefaultAsync(d => d.Id == request.DecisionId, ct);
        if (decision is null)
        {
            return Result.Fail("Decision not found", ErrorCodes.NotFound);
        }

        var now = DateTime.UtcNow;
        if (!decision.CanRevert(now))
        {
            return Result.Fail("Only an auto-approved decision can be reverted, within 15 minutes of running.");
        }

        var undone = await UndoAsync(decision, ct);
        if (!undone.IsSuccess)
        {
            return undone;
        }

        decision.MarkReverted(now);
        await tenantUow.SaveChangesAsync(ct);

        var tenantId = tenantUow.GetCurrentTenant().Id;
        var conversation = await notes.LoadConversationAsync(decision, ct);
        await notes.AppendAsync(
            conversation, AgentDecisionNote.Reverted(decision), currentUser.GetUserId() ?? Guid.Empty,
            message => broadcastService.BroadcastMessageAsync(tenantId, message), ct);

        return Result.Ok();
    }

    private async Task<Result> UndoAsync(AgentDecision decision, CancellationToken ct)
    {
        return decision.Type switch
        {
            AgentDecisionType.AssignLoad when decision.LoadId is { } loadId =>
                await mediator.Send(new AssignLoadToTruckCommand { LoadId = loadId, TruckId = null }, ct),
            AgentDecisionType.CreateTrip when decision.TripId is { } tripId => await DeleteDraftTripAsync(tripId, ct),
            AgentDecisionType.CreateInvoice when decision.InvoiceId is { } invoiceId =>
                await DeleteDraftInvoiceAsync(invoiceId, ct),
            _ => Result.Fail("This decision has nothing recorded to revert.")
        };
    }

    private async Task<Result> DeleteDraftTripAsync(Guid tripId, CancellationToken ct)
    {
        var trip = await tenantUow.Repository<Trip>().GetByIdAsync(tripId, ct);
        if (trip is null)
        {
            return Result.Ok();
        }

        if (trip.Status != TripStatus.Draft)
        {
            return Result.Fail("The trip was already dispatched, so it can't be reverted.");
        }

        return await mediator.Send(new DeleteTripCommand { Id = tripId }, ct);
    }

    private async Task<Result> DeleteDraftInvoiceAsync(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(invoiceId, ct);
        if (invoice is null)
        {
            return Result.Ok();
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return Result.Fail("The invoice is no longer a draft, so it can't be reverted.");
        }

        return await mediator.Send(new DeleteInvoiceCommand { Id = invoiceId }, ct);
    }
}
