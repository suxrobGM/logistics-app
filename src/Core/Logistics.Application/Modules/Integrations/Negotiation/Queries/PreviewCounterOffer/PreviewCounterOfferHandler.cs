using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

internal sealed class PreviewCounterOfferHandler(
    ITenantUnitOfWork tenantUow,
    INegotiationEmailComposer composer,
    IThreadedEmailSender emailSender)
    : IAppRequestHandler<PreviewCounterOfferQuery, Result<CounterOfferPreviewDto>>
{
    private const string ToolName = "propose_counter_offer";

    public async Task<Result<CounterOfferPreviewDto>> Handle(PreviewCounterOfferQuery req, CancellationToken ct)
    {
        var decision = await tenantUow.Repository<AgentDecision>().GetByIdAsync(req.DecisionId, ct);
        if (decision is null)
        {
            return Result<CounterOfferPreviewDto>.Fail($"Could not find a decision with ID '{req.DecisionId}'");
        }

        if (decision.ToolName != ToolName || string.IsNullOrWhiteSpace(decision.ToolInput))
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
        var currency = listing.TotalRate?.Currency ?? ComposeNegotiationEmailRequest.DefaultCurrency;

        var composed = await composer.ComposeAsync(ComposeNegotiationEmailRequest.For(
            listing, tenant, input.ProposedTotalRate, input.ProposedRatePerMile,
            input.Message, replyToAddress), ct);

        return Result<CounterOfferPreviewDto>.Ok(new CounterOfferPreviewDto
        {
            Subject = composed.Subject,
            Message = composed.SanitizedMessage,
            ToEmail = listing.BrokerEmail ?? "",
            ReplyToAddress = replyToAddress,
            ProposedTotalRate = input.ProposedTotalRate,
            ProposedRatePerMile = input.ProposedRatePerMile,
            Currency = currency
        });
    }

    /// <summary>
    /// <c>ToolInput</c> is the model's own JSON, not what the tool bound, so it is read leniently.
    /// A strict reader rejects the numbers-as-strings the models actually emit, and the preview
    /// would then fail on offers the tool itself accepted.
    /// </summary>
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
            root.GetDecimal("proposed_rate_per_mile"),
            root.GetString("message") ?? "");

        return true;
    }

    private readonly record struct CounterOfferInput(
        Guid ListingId,
        decimal ProposedTotalRate,
        decimal? ProposedRatePerMile,
        string Message);
}
