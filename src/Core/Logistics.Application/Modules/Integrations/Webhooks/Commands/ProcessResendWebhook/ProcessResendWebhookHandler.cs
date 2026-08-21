using System.Text.Json;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Negotiation;
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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

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

        var eventKey = payload.Data?.EmailId ?? req.SvixId!;
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

        if (route is null || !route.AcceptsMailAt(DateTime.UtcNow))
        {
            logger.LogInformation(
                "Reply token on Resend webhook {EventKey} is unknown, revoked, or past its window", eventKey);
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
            ProviderEmailId = payload.Data?.EmailId ?? eventKey,
            From = payload.Data?.From ?? "",
            Subject = payload.Data?.Subject,
            MessageId = payload.Data?.MessageId
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
    /// <c>received_for</c> holds the address the mail was actually delivered to, which survives
    /// forwarding; <c>to</c> is checked too because a direct reply lands there.
    /// </summary>
    private static string? ExtractThreadToken(ResendEvent payload) =>
        (payload.Data?.ReceivedFor ?? []).Concat(payload.Data?.To ?? [])
            .Select(NegotiationReplyAddress.TryParseToken)
            .FirstOrDefault(token => token is not null);

    private static bool TryReadEvent(string rawBody, out ResendEvent payload)
    {
        try
        {
            payload = JsonSerializer.Deserialize<ResendEvent>(rawBody, JsonOptions) ?? new ResendEvent();
            return payload.Type is not null;
        }
        catch (JsonException)
        {
            payload = new ResendEvent();
            return false;
        }
    }

    private sealed record ResendEvent
    {
        public string? Type { get; init; }
        public ResendEventData? Data { get; init; }
    }

    private sealed record ResendEventData
    {
        public string? EmailId { get; init; }
        public string? From { get; init; }
        public string? Subject { get; init; }
        public string? MessageId { get; init; }
        public List<string>? To { get; init; }
        public List<string>? ReceivedFor { get; init; }
    }
}
