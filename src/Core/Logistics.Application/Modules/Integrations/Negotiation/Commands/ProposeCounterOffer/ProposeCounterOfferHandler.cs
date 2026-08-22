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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class ProposeCounterOfferHandler(
    ITenantUnitOfWork tenantUow,
    IInboundEmailRouteRegistry routeRegistry,
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

        var brokerEmail = listing.BrokerEmail;
        if (string.IsNullOrWhiteSpace(brokerEmail))
        {
            return Result<RateNegotiationDto>.Fail(
                "This listing has no broker email address, so a counter-offer cannot be sent.");
        }

        var negotiationRepo = tenantUow.Repository<RateNegotiation>();
        var negotiation = await negotiationRepo.GetAsync(RateNegotiation.OpenForListing(listing.Id), ct);

        var isNewThread = negotiation is null;
        var currency = ListingCurrency.Of(listing);

        // Floor first: it is a local read that rejects most bad offers, whereas the credit gate
        // costs a vendor API call and a write. Round 1 resolves the lane floor and freezes it on the
        // thread; every later round is checked against that snapshot, so an edit to the lane floor
        // cannot move the bar mid-negotiation - and the booking check reads the same number.
        var floor = negotiation is null
            ? await floorResolver.ResolveAsync(listing, ct)
            : negotiation.ToEffectiveFloor();

        if (FloorRejection(req, floor, listing) is { } rejection)
        {
            return Result<RateNegotiationDto>.Fail(rejection.Error, rejection.Code);
        }

        var creditGate = await BrokerCreditGate.EvaluateAsync(
            tenantUow, brokerCreditService, listing, overrideCheck: false, ct);

        if (!creditGate.IsSuccess)
        {
            return Result<RateNegotiationDto>.Fail(creditGate.Error!, creditGate.ErrorCode!);
        }

        if (negotiation is null)
        {
            negotiation = RateNegotiation.Create(
                listing.Id,
                brokerEmail,
                floor.ToSnapshot(currency),
                listing.BrokerName,
                listing.BrokerMcNumber,
                req.ConversationId);
        }
        else if (!negotiation.CanCounter)
        {
            return Result<RateNegotiationDto>.Fail(
                $"This negotiation already used all {RateNegotiation.MaxRounds} rounds. Close it or book at the broker's last offer.");
        }

        // Projected rather than read off the navigation: only the header ids are needed, and the
        // rows carry up to 64KB of stored body each.
        var priorMessageIds = await tenantUow.Repository<NegotiationMessage>().Query()
            .Where(m => m.NegotiationId == negotiation.Id && m.RfcMessageId != null)
            .OrderBy(m => m.Sequence)
            .Select(m => m.RfcMessageId!)
            .ToArrayAsync(ct);

        var replyToAddress = NegotiationReplyAddress.Format(negotiation.ReplyToken, emailSender.ReplyDomain);
        var messageId = NegotiationMessageId.Create(emailSender.ReplyDomain);
        var tenant = tenantUow.GetCurrentTenant();

        var composed = await composer.ComposeAsync(ComposeNegotiationEmailRequest.For(
            listing, tenant, req.ProposedTotalRate, req.ProposedRatePerMile, req.Message, replyToAddress), ct);

        // The broker can reply the moment the mail lands, so a new thread's route has to exist
        // before the send - opening it afterwards drops any reply that beats the save.
        if (isNewThread)
        {
            await routeRegistry.OpenAsync(negotiation.ReplyToken, tenant.Id, negotiation.ExpiresAt, ct);
        }

        var sendResult = await emailSender.SendAsync(new ThreadedEmail(
            To: brokerEmail,
            Subject: composed.Subject,
            HtmlBody: composed.HtmlBody,
            ReplyTo: replyToAddress,
            MessageId: messageId,
            InReplyToMessageId: priorMessageIds.LastOrDefault(),
            References: priorMessageIds.Length > 0 ? string.Join(' ', priorMessageIds) : null), ct);

        // Nothing is persisted unless the broker actually got the mail, so a failed send leaves the
        // thread exactly as it was and the agent can be asked to try again.
        if (!sendResult.Success)
        {
            if (isNewThread)
            {
                await routeRegistry.RevokeAsync([negotiation.ReplyToken], ct);
            }

            return Result<RateNegotiationDto>.Fail(
                "Could not send the counter-offer email to the broker. Nothing was changed.");
        }

        var message = negotiation.AddOutboundMessage(
            textBody: composed.SanitizedMessage,
            subject: composed.Subject,
            proposedTotalRate: new Money { Amount = req.ProposedTotalRate, Currency = currency },
            proposedRatePerMile: req.ProposedRatePerMile,
            agentDecisionId: req.DecisionId);

        message.RfcMessageId = messageId;
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

        // The route's expiry tracks the thread's reply window, so it is restamped on every send.
        await routeRegistry.RefreshAsync(negotiation.ReplyToken, tenant.Id, negotiation.ExpiresAt, ct);

        logger.LogInformation(
            "Sent counter-offer round {Round} on negotiation {NegotiationId} for listing {ListingId}",
            negotiation.RoundCount, negotiation.Id, listing.Id);

        var dto = negotiation.ToDto(listing);
        await broadcastService.BroadcastNegotiationAsync(tenant.Id, dto);
        return Result<RateNegotiationDto>.Ok(dto);
    }

    /// <summary>
    /// Why this offer may not be sent, or <c>null</c> when it clears the floor.
    /// </summary>
    private static (string Error, string Code)? FloorRejection(
        ProposeCounterOfferCommand req, EffectiveRateFloorDto floor, LoadBoardListing listing)
    {
        var lane = $"{listing.OriginAddress.State} to {listing.DestinationAddress.State}";

        if (!floor.HasFloor)
        {
            return (
                $"No rate floor covers {lane} and your company has no default floor, so this offer cannot be checked. " +
                "Add a lane rate floor before negotiating.",
                ErrorCodes.NegotiationFloorMissing);
        }

        if (floor.EffectiveFloorTotal is { } floorTotal)
        {
            return req.ProposedTotalRate < floorTotal
                ? ($"The offer of {req.ProposedTotalRate:N2} is below your floor of {floorTotal:N2} for {lane}.",
                    ErrorCodes.NegotiationBelowFloor)
                : null;
        }

        if (req.ProposedRatePerMile is { } perMile && floor.MinRatePerMile is { } minPerMile)
        {
            return perMile < minPerMile
                ? ($"The offer of {perMile:N2} per mile is below your floor of {minPerMile:N2} per mile for {lane}.",
                    ErrorCodes.NegotiationBelowFloor)
                : null;
        }

        return (
            $"The listing has no distance and your floor for {lane} is per-mile only, so this offer cannot be checked. " +
            "Set a minimum total rate on the lane floor, or offer a per-mile rate.",
            ErrorCodes.NegotiationFloorMissing);
    }
}
