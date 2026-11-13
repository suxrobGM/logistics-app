using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Modules.AI.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

internal sealed class PreviewCounterOfferHandler(
    ITenantUnitOfWork tenantUow,
    INegotiationEmailComposer composer,
    IThreadedEmailSender emailSender)
    : IRequestHandler<PreviewCounterOfferQuery, Result<CounterOfferPreviewDto>>
{
    public async Task<Result<CounterOfferPreviewDto>> Handle(PreviewCounterOfferQuery req, CancellationToken ct)
    {
        var decision = await tenantUow.Repository<AgentDecision>().GetByIdAsync(req.DecisionId, ct);
        if (decision is null)
        {
            return Result<CounterOfferPreviewDto>.Fail($"Could not find a decision with ID '{req.DecisionId}'", ErrorCodes.NotFound);
        }

        if (decision.ToolName != AgentToolNames.ProposeCounterOffer || string.IsNullOrWhiteSpace(decision.ToolInput))
        {
            return Result<CounterOfferPreviewDto>.Fail("This decision does not carry a counter-offer email.");
        }

        if (!TryReadInput(decision.ToolInput, out var input))
        {
            return Result<CounterOfferPreviewDto>.Fail("The decision's tool input could not be read.");
        }

        var listing = await tenantUow.Repository<LoadBoardListing>().GetByIdAsync(input.ListingId, ct);
        if (listing is null)
        {
            return Result<CounterOfferPreviewDto>.Fail("The listing this offer refers to no longer exists.");
        }

        var negotiation = await tenantUow.Repository<RateNegotiation>()
            .GetAsync(RateNegotiation.OpenForListing(listing.Id), ct);

        // A first-round offer has no thread yet, so there is no reply token to show. The address is
        // display-only here - the template body never contains it, so the preview still matches.
        var replyToAddress = NegotiationReplyAddress.Format(
            negotiation?.ReplyToken ?? NegotiationReplyAddress.UnassignedToken, emailSender.ReplyDomain);

        var tenant = tenantUow.GetCurrentTenant();
        var currency = ListingCurrency.Get(listing, tenant);
        var proposedRatePerKm = input.ProposedRatePerDistance is { } rate
            ? tenant.Settings.DistanceUnit.RateToPerKm(rate)
            : (decimal?)null;

        var composed = await composer.ComposeAsync(ComposeNegotiationEmailRequest.For(
            listing, tenant, input.ProposedTotalRate, proposedRatePerKm, input.Message, replyToAddress), ct);

        return Result<CounterOfferPreviewDto>.Ok(new CounterOfferPreviewDto
        {
            Subject = composed.Subject,
            Message = composed.SanitizedMessage,
            ToEmail = listing.BrokerEmail ?? "",
            ReplyToAddress = replyToAddress,
            ProposedTotalRate = input.ProposedTotalRate,
            ProposedRatePerKm = proposedRatePerKm,
            Currency = currency
        });
    }

    /// <summary>Parses the counter-offer tool input. Numbers written as strings are accepted, since models often emit them that way.</summary>
    private static bool TryReadInput(string toolInput, out CounterOfferInput input)
    {
        input = default;

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(toolInput);
        }
        catch (JsonException)
        {
            return false;
        }

        if (root is null ||
            root.GetGuid("listing_id") is not { } listingId ||
            root.GetDecimal("proposed_total_rate") is not { } totalRate)
        {
            return false;
        }

        input = new CounterOfferInput(
            listingId,
            totalRate,
            root.GetDecimal("proposed_rate_per_distance"),
            root.GetString("message") ?? "");

        return true;
    }

    private readonly record struct CounterOfferInput(
        Guid ListingId,
        decimal ProposedTotalRate,
        decimal? ProposedRatePerDistance,
        string Message);
}
