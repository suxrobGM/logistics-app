using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class ProposeCounterOfferHandler(
    ITenantUnitOfWork tenantUow,
    IMasterUnitOfWork masterUow,
    IBrokerCreditService brokerCreditService,
    ILaneRateFloorResolver floorResolver,
    INegotiationEmailComposer composer,
    IThreadedEmailSender emailSender,
    IAIDispatchBroadcastService broadcastService,
    ILogger<ProposeCounterOfferHandler> logger)
    : IAppRequestHandler<ProposeCounterOfferCommand, Result<RateNegotiationDto>>
{
    public async Task<Result<RateNegotiationDto>> Handle(ProposeCounterOfferCommand req, CancellationToken ct)
    {
        if (req.ProposedTotalRate <= 0)
        {
            return Result<RateNegotiationDto>.Fail("The counter-offer must be greater than zero.");
        }

        var listing = await tenantUow.Repository<LoadBoardListing>().GetByIdAsync(req.ListingId, ct);
        if (listing is null)
        {
            return Result<RateNegotiationDto>.Fail("Load board listing not found");
        }

        if (listing.Status != LoadBoardListingStatus.Available)
        {
            return Result<RateNegotiationDto>.Fail(
                $"Load board listing is not available (current status: {listing.Status})");
        }

        if (string.IsNullOrWhiteSpace(listing.BrokerEmail))
        {
            return Result<RateNegotiationDto>.Fail(
                "This listing has no broker email address, so a counter-offer cannot be sent.");
        }

        var creditGate = await BrokerCreditGate.EvaluateAsync(
            tenantUow, brokerCreditService, listing, overrideCheck: false, ct);

        if (!creditGate.IsSuccess)
        {
            return Result<RateNegotiationDto>.Fail(creditGate.Error!, creditGate.ErrorCode!);
        }

        var floor = await floorResolver.ResolveAsync(listing, ct);
        var floorCheck = CheckAgainstFloor(req, floor, listing);
        if (!floorCheck.IsSuccess)
        {
            return Result<RateNegotiationDto>.Fail(floorCheck.Error!, floorCheck.ErrorCode!);
        }

        var negotiationRepo = tenantUow.Repository<RateNegotiation>();
        var negotiation = await negotiationRepo.GetAsync(
            n => n.LoadBoardListingId == listing.Id &&
                 (n.Status == RateNegotiationStatus.AwaitingBroker ||
                  n.Status == RateNegotiationStatus.BrokerReplied), ct);

        var isNewThread = negotiation is null;
        var currency = listing.TotalRate?.Currency ?? "USD";

        if (negotiation is null)
        {
            negotiation = RateNegotiation.Create(
                listing.Id, listing.BrokerEmail!, listing.BrokerName, listing.BrokerMcNumber, req.ConversationId);

            negotiation.FloorRatePerMile = floor.MinRatePerMile;
            negotiation.FloorTotalRate = floor.MinTotalRate ?? (floor.EffectiveFloorTotal is { } total
                ? new Money { Amount = total, Currency = currency }
                : null);
            negotiation.FloorSource = floor.Source;
        }
        else if (negotiation.RoundCount >= RateNegotiation.MaxRounds)
        {
            return Result<RateNegotiationDto>.Fail(
                $"This negotiation already used all {RateNegotiation.MaxRounds} rounds. Close it or book at the broker's last offer.");
        }

        var priorMessageIds = negotiation.Messages
            .OrderBy(m => m.Sequence)
            .Select(m => m.ProviderMessageId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToArray();

        var replyToAddress = $"offer-{negotiation.ReplyToken}@{emailSender.ReplyDomain}";
        var tenant = tenantUow.GetCurrentTenant();

        var composed = await composer.ComposeAsync(new ComposeNegotiationEmailRequest(
            OriginCity: listing.OriginAddress.City,
            OriginState: listing.OriginAddress.State,
            DestinationCity: listing.DestinationAddress.City,
            DestinationState: listing.DestinationAddress.State,
            PickupDate: listing.PickupDateStart ?? listing.ExpiresAt,
            EquipmentType: listing.EquipmentType ?? "Not specified",
            OfferAmount: req.ProposedTotalRate,
            Currency: currency,
            OfferPerMile: req.ProposedRatePerMile,
            AgentMessage: req.Message,
            CompanyName: tenant.CompanyName ?? tenant.Name,
            CompanyMcNumber: tenant.McNumber,
            ThreadReference: negotiation.Reference,
            ReplyToAddress: replyToAddress,
            BrokerName: listing.BrokerName), ct);

        var sendResult = await emailSender.SendAsync(new ThreadedEmail(
            To: listing.BrokerEmail!,
            Subject: composed.Subject,
            HtmlBody: composed.HtmlBody,
            ReplyTo: replyToAddress,
            InReplyToMessageId: priorMessageIds.LastOrDefault(),
            References: priorMessageIds.Length > 0 ? string.Join(' ', priorMessageIds) : null), ct);

        // Nothing is persisted unless the broker actually got the mail, so a failed send leaves the
        // thread exactly as it was and the agent can be asked to try again.
        if (!sendResult.Success)
        {
            return Result<RateNegotiationDto>.Fail(
                "Could not send the counter-offer email to the broker. Nothing was changed.");
        }

        var message = negotiation.AddOutboundMessage(
            textBody: composed.SanitizedMessage,
            subject: composed.Subject,
            proposedTotalRate: new Money { Amount = req.ProposedTotalRate, Currency = currency },
            proposedRatePerMile: req.ProposedRatePerMile,
            agentDecisionId: req.DecisionId);

        message.ProviderMessageId = sendResult.ProviderMessageId;
        message.InReplyToMessageId = priorMessageIds.LastOrDefault();

        if (req.ConversationId.HasValue)
        {
            negotiation.ConversationId = req.ConversationId;
        }

        if (isNewThread)
        {
            await negotiationRepo.AddAsync(negotiation, ct);
        }

        await tenantUow.Repository<NegotiationMessage>().AddAsync(message, ct);

        if (req.DecisionId.HasValue)
        {
            var decision = await tenantUow.Repository<AgentDecision>().GetByIdAsync(req.DecisionId.Value, ct);
            if (decision is not null)
            {
                decision.NegotiationId = negotiation.Id;
            }
        }

        await tenantUow.SaveChangesAsync(ct);
        await UpsertInboundRouteAsync(negotiation, tenant.Id, isNewThread, ct);

        logger.LogInformation(
            "Sent counter-offer round {Round} on negotiation {NegotiationId} for listing {ListingId}",
            negotiation.RoundCount, negotiation.Id, listing.Id);

        var dto = negotiation.ToDto(listing);
        await broadcastService.BroadcastNegotiationAsync(tenant.Id, dto);
        return Result<RateNegotiationDto>.Ok(dto);
    }

    private static Result CheckAgainstFloor(
        ProposeCounterOfferCommand req, EffectiveRateFloorDto floor, LoadBoardListing listing)
    {
        var lane = $"{listing.OriginAddress.State} to {listing.DestinationAddress.State}";

        if (!floor.HasFloor)
        {
            return Result.Fail(
                $"No rate floor covers {lane} and your company has no default floor, so this offer cannot be checked. " +
                "Add a lane rate floor before negotiating.",
                ErrorCodes.NegotiationFloorMissing);
        }

        if (floor.EffectiveFloorTotal is { } floorTotal)
        {
            return req.ProposedTotalRate < floorTotal
                ? Result.Fail(
                    $"The offer of {req.ProposedTotalRate:N2} is below your floor of {floorTotal:N2} for {lane}.",
                    ErrorCodes.NegotiationBelowFloor)
                : Result.Ok();
        }

        if (req.ProposedRatePerMile is { } perMile && floor.MinRatePerMile is { } minPerMile)
        {
            return perMile < minPerMile
                ? Result.Fail(
                    $"The offer of {perMile:N2} per mile is below your floor of {minPerMile:N2} per mile for {lane}.",
                    ErrorCodes.NegotiationBelowFloor)
                : Result.Ok();
        }

        return Result.Fail(
            $"The listing has no distance and your floor for {lane} is per-mile only, so this offer cannot be checked. " +
            "Set a minimum total rate on the lane floor, or offer a per-mile rate.",
            ErrorCodes.NegotiationFloorMissing);
    }

    /// <summary>
    /// The master-database route is what lets inbound mail find this tenant. Its expiry tracks the
    /// thread's reply window, so it is refreshed on every send, not just the first.
    /// </summary>
    private async Task UpsertInboundRouteAsync(
        RateNegotiation negotiation, Guid tenantId, bool isNewThread, CancellationToken ct)
    {
        var routeRepo = masterUow.Repository<InboundEmailRoute>();

        if (isNewThread)
        {
            await routeRepo.AddAsync(new InboundEmailRoute
            {
                ThreadToken = negotiation.ReplyToken,
                TenantId = tenantId,
                Purpose = InboundEmailPurpose.RateNegotiation,
                ExpiresAt = negotiation.ExpiresAt
            }, ct);
        }
        else
        {
            var route = await routeRepo.GetAsync(r => r.ThreadToken == negotiation.ReplyToken, ct);
            if (route is null)
            {
                return;
            }

            route.ExpiresAt = negotiation.ExpiresAt;
        }

        await masterUow.SaveChangesAsync(ct);
    }
}
