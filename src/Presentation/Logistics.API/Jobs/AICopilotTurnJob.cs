using Hangfire;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that runs a single copilot turn. Enqueued by <see cref="HangfireAICopilotTurnRunner"/>
///     from the send-message handler, which has already created the user message and marked the
///     conversation Running.
/// </summary>
public class AICopilotTurnJob(
    IServiceScopeFactory scopeFactory,
    ILogger<AICopilotTurnJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        // Hangfire bypasses the MediatR pipeline, so [RequiresFeature] on the send command does
        // not protect this path - re-check explicitly.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.AICopilot))
        {
            logger.LogWarning(
                "AICopilot feature is disabled for tenant {TenantId}; skipping turn for conversation {ConversationId}",
                tenantId, conversationId);
            return;
        }

        var copilotService = scope.ServiceProvider.GetRequiredService<IAICopilotService>();

        try
        {
            await copilotService.RunTurnAsync(new AICopilotTurnRequest(tenantId, conversationId, userId), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Copilot turn failed for conversation {ConversationId} (tenant {TenantId})",
                conversationId, tenantId);
        }
    }
}

/// <summary>
///     Hangfire-backed implementation of <see cref="IBackgroundJobRunner{T}"/> for copilot turns.
/// </summary>
public class HangfireAICopilotTurnRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<AICopilotTurnRequest>
{
    public void Enqueue(AICopilotTurnRequest request)
    {
        jobClient.Enqueue<AICopilotTurnJob>(job => job.RunAsync(
            request.TenantId,
            request.ConversationId,
            request.UserId,
            CancellationToken.None));
    }
}
