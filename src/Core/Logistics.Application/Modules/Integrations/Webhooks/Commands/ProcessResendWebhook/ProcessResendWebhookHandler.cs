using System.Text.Json;
using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Modules.Integrations.Negotiation;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Application.Modules.Integrations.Webhooks.Services;
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
    IWebhookEventTracker webhookEvents,
    ILogger<ProcessResendWebhookHandler> logger)
    : IAppRequestHandler<ProcessResendWebhookCommand, Result>
{
    private const string Provider = "Resend";
    private const string ReceivedEventType = "email.received";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result> Handle(ProcessResendWebhookCommand req, CancellationToken ct)
    {
        if (!verifier.Verify(req.RawBody, req.SvixId, req.SvixTimestamp, req.SvixSignature))
        {
            logger.LogWarning("Resend webhook signature verification failed");
            return Result.Fail("Resend webhook signature verification failed", ErrorCodes.WebhookRejected);
        }

        if (!TryReadEvent(req.RawBody, out var payload))
        {
            logger.LogWarning("Resend webhook body could not be parsed");
            return Result.Fail("Resend webhook body could not be parsed", ErrorCodes.WebhookRejected);
        }

        if (payload.Type != ReceivedEventType)
        {
            logger.LogInformation("Ignoring Resend webhook of type {Type}", payload.Type);
            return Result.Ok();
        }

        var eventKey = payload.Data?.EmailId ?? req.SvixId!;
        if (await webhookEvents.WasAlreadyHandledAsync(Provider, eventKey, ct))
        {
            logger.LogInformation("Duplicate Resend webhook '{EventKey}' ignored", eventKey);
            return Result.Ok();
        }

        if (ExtractThreadToken(payload) is not { } token)
        {
            logger.LogInformation("Resend webhook {EventKey} is not addressed to a negotiation thread", eventKey);
            return Result.Ok();
        }

        var route = await masterUow.Repository<InboundEmailRoute>()
            .GetAsync(r => r.ThreadToken == token, ct);

        if (route is null || !route.AcceptsMailAt(DateTime.UtcNow))
        {
            logger.LogInformation(
                "Reply token on Resend webhook {EventKey} is unknown, revoked, or past its window", eventKey);
            return Result.Ok();
        }

        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(route.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not open tenant {TenantId} for Resend webhook {EventKey}",
                route.TenantId, eventKey);
            return Result.Fail($"Could not open tenant {route.TenantId} for Resend webhook {eventKey}");
        }

        var inner = await mediator.Send(new ProcessInboundNegotiationEmailCommand
        {
            ThreadToken = token,
            ProviderEmailId = payload.Data?.EmailId ?? eventKey,
            From = payload.Data?.From ?? "",
            Subject = payload.Data?.Subject,
            MessageId = payload.Data?.MessageId
        }, ct);

        // Record last: a transient failure must stay retryable, and a recorded key would kill the retry.
        if (!inner.IsSuccess)
        {
            logger.LogWarning("Resend webhook {EventKey} could not be processed yet: {Error}",
                eventKey, inner.Error);
            return Result.Fail(inner.Error ?? $"Resend webhook {eventKey} could not be processed yet");
        }

        await webhookEvents.MarkHandledAsync(Provider, eventKey, ct);

        return Result.Ok();
    }

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
