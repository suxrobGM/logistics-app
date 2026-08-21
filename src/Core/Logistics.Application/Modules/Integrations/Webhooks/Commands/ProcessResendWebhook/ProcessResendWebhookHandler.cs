using System.Text.Json;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Webhooks.Commands;

internal sealed class ProcessResendWebhookHandler(
    IInboundEmailWebhookVerifier verifier,
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IMediator mediator,
    ILogger<ProcessResendWebhookHandler> logger)
    : IAppRequestHandler<ProcessResendWebhookCommand, Result<ResendWebhookOutcome>>
{
    private const string Provider = "Resend";
    private const string ReceivedEventType = "email.received";
    private const string ReplyAddressPrefix = "offer-";

    public async Task<Result<ResendWebhookOutcome>> Handle(
        ProcessResendWebhookCommand req, CancellationToken ct)
    {
        if (!verifier.Verify(req.RawBody, req.SvixId, req.SvixTimestamp, req.SvixSignature))
        {
            logger.LogWarning("Resend webhook signature verification failed");
            return Ok(ResendWebhookOutcome.BadSignature);
        }

        if (!TryReadEvent(req.RawBody, out var payload))
        {
            logger.LogWarning("Resend webhook body could not be parsed");
            return Ok(ResendWebhookOutcome.BadSignature);
        }

        if (payload.Type != ReceivedEventType)
        {
            logger.LogInformation("Ignoring Resend webhook of type {Type}", payload.Type);
            return Ok(ResendWebhookOutcome.Accepted);
        }

        var eventKey = payload.EmailId ?? req.SvixId!;
        var ledger = masterUow.Repository<ProcessedWebhookEvent>();

        if (await ledger.GetAsync(e => e.Provider == Provider && e.EventKey == eventKey, ct) is not null)
        {
            logger.LogInformation("Duplicate Resend webhook '{EventKey}' ignored", eventKey);
            return Ok(ResendWebhookOutcome.Accepted);
        }

        if (ExtractThreadToken(payload) is not { } token)
        {
            logger.LogInformation("Resend webhook {EventKey} is not addressed to a negotiation thread", eventKey);
            return Ok(ResendWebhookOutcome.Accepted);
        }

        var route = await masterUow.Repository<InboundEmailRoute>()
            .GetAsync(r => r.ThreadToken == token, ct);

        if (route is null || route.RevokedAt is not null || route.Purpose != InboundEmailPurpose.RateNegotiation)
        {
            logger.LogInformation("Reply token on Resend webhook {EventKey} is unknown or revoked", eventKey);
            return Ok(ResendWebhookOutcome.Accepted);
        }

        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(route.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not open tenant {TenantId} for Resend webhook {EventKey}",
                route.TenantId, eventKey);
            return Ok(ResendWebhookOutcome.Transient);
        }

        var inner = await mediator.Send(new ProcessInboundNegotiationEmailCommand
        {
            ThreadToken = token,
            ProviderEmailId = payload.EmailId ?? eventKey,
            From = payload.From ?? "",
            Subject = payload.Subject,
            MessageId = payload.MessageId
        }, ct);

        // Ledger last: a transient failure must stay retryable, and a recorded key would kill the retry.
        if (!inner.IsSuccess)
        {
            logger.LogWarning("Resend webhook {EventKey} could not be processed yet: {Error}",
                eventKey, inner.Error);
            return Ok(ResendWebhookOutcome.Transient);
        }

        await ledger.AddAsync(new ProcessedWebhookEvent { Provider = Provider, EventKey = eventKey }, ct);
        await masterUow.SaveChangesAsync(ct);

        return Ok(ResendWebhookOutcome.Accepted);
    }

    private static Result<ResendWebhookOutcome> Ok(ResendWebhookOutcome outcome) =>
        Result<ResendWebhookOutcome>.Ok(outcome);

    /// <summary>
    /// The thread token rides in the local part of the reply address. <c>received_for</c> holds the
    /// address the mail was actually delivered to, which survives forwarding; <c>to</c> is checked
    /// too because a direct reply lands there.
    /// </summary>
    private static string? ExtractThreadToken(ResendEvent payload)
    {
        var candidates = (payload.ReceivedFor ?? []).Concat(payload.To ?? []);

        foreach (var address in candidates)
        {
            var localPart = address.Split('@')[0].Trim();
            if (localPart.StartsWith(ReplyAddressPrefix, StringComparison.OrdinalIgnoreCase) &&
                localPart.Length > ReplyAddressPrefix.Length)
            {
                return localPart[ReplyAddressPrefix.Length..];
            }
        }

        return null;
    }

    private static bool TryReadEvent(string rawBody, out ResendEvent payload)
    {
        payload = new ResendEvent();

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            payload.Type = root.TryGetProperty("type", out var type) ? type.GetString() : null;

            if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                payload.EmailId = data.TryGetProperty("email_id", out var id) ? id.GetString() : null;
                payload.From = data.TryGetProperty("from", out var from) ? from.GetString() : null;
                payload.Subject = data.TryGetProperty("subject", out var subject) ? subject.GetString() : null;
                payload.MessageId = data.TryGetProperty("message_id", out var mid) ? mid.GetString() : null;
                payload.To = ReadStrings(data, "to");
                payload.ReceivedFor = ReadStrings(data, "received_for");
            }

            return payload.Type is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static List<string> ReadStrings(JsonElement data, string property) =>
        data.TryGetProperty(property, out var array) && array.ValueKind == JsonValueKind.Array
            ? [.. array.EnumerateArray().Select(e => e.GetString()).OfType<string>()]
            : [];

    private sealed class ResendEvent
    {
        public string? Type { get; set; }
        public string? EmailId { get; set; }
        public string? From { get; set; }
        public string? Subject { get; set; }
        public string? MessageId { get; set; }
        public List<string>? To { get; set; }
        public List<string>? ReceivedFor { get; set; }
    }
}
