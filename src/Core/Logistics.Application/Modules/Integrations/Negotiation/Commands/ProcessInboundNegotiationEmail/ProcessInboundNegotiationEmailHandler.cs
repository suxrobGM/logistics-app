using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

internal sealed class ProcessInboundNegotiationEmailHandler(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    IInboundEmailReader inboundEmailReader,
    INegotiationTurnStarter turnStarter,
    IAIDispatchBroadcastService broadcastService,
    ILogger<ProcessInboundNegotiationEmailHandler> logger)
    : IAppRequestHandler<ProcessInboundNegotiationEmailCommand, Result>
{
    private const int MaxRawBodyChars = 64 * 1024;

    public async Task<Result> Handle(ProcessInboundNegotiationEmailCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AIRateNegotiation))
        {
            logger.LogInformation(
                "Rate negotiation is disabled for tenant {TenantId}; dropping inbound reply", tenant.Id);
            return Result.Ok();
        }

        var negotiation = await tenantUow.Repository<RateNegotiation>()
            .GetAsync(n => n.ReplyToken == req.ThreadToken, ct);

        if (negotiation is null)
        {
            logger.LogInformation("No negotiation matches the reply token on inbound email {EmailId}",
                req.ProviderEmailId);
            return Result.Ok();
        }

        if (negotiation.Status is not (RateNegotiationStatus.AwaitingBroker or RateNegotiationStatus.BrokerReplied))
        {
            logger.LogInformation(
                "Negotiation {NegotiationId} is {Status}; inbound reply ignored",
                negotiation.Id, negotiation.Status);
            return Result.Ok();
        }

        var senderMatches = AddressesMatch(req.From, negotiation.BrokerEmail);

        // The body is a second call: the webhook carries metadata only. A failure here is transient,
        // so nothing is written and the provider gets a retryable answer.
        var email = await inboundEmailReader.GetAsync(req.ProviderEmailId, ct);
        if (email is null)
        {
            return Result.Fail($"Could not fetch the body of inbound email '{req.ProviderEmailId}'");
        }

        var rawBody = Clamp(email.TextBody ?? email.HtmlBody ?? "");
        var strippedText = EmailReplyParser.Strip(email.TextBody ?? email.HtmlBody ?? "");

        var message = negotiation.AddInboundMessage(
            textBody: senderMatches ? strippedText : "",
            subject: email.Subject ?? req.Subject,
            rawBody: rawBody,
            providerMessageId: email.MessageId ?? req.MessageId,
            quarantined: !senderMatches);

        await tenantUow.Repository<NegotiationMessage>().AddAsync(message, ct);
        await tenantUow.SaveChangesAsync(ct);

        var listing = await tenantUow.Repository<LoadBoardListing>()
            .GetByIdAsync(negotiation.LoadBoardListingId, ct);
        await broadcastService.BroadcastNegotiationAsync(tenant.Id, negotiation.ToDto(listing));

        if (!senderMatches)
        {
            logger.LogWarning(
                "Inbound email {EmailId} for negotiation {NegotiationId} came from {From}, not the broker; quarantined",
                req.ProviderEmailId, negotiation.Id, req.From);
            return Result.Ok();
        }

        await turnStarter.NotifyBrokerReplyAsync(negotiation, strippedText, ct);
        return Result.Ok();
    }

    /// <summary>
    /// Compares only the addr-spec: display names are attacker-controlled and vary between replies.
    /// </summary>
    private static bool AddressesMatch(string? left, string? right) =>
        ExtractAddress(left) is { Length: > 0 } a &&
        ExtractAddress(right) is { Length: > 0 } b &&
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static string ExtractAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var start = value.IndexOf('<');
        var end = value.IndexOf('>');
        var address = start >= 0 && end > start ? value[(start + 1)..end] : value;
        return address.Trim();
    }

    private static string Clamp(string text) =>
        text.Length <= MaxRawBodyChars ? text : text[..MaxRawBodyChars];
}
