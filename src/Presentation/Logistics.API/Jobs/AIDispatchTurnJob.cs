using Hangfire;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>Background job that runs one dispatch agent turn. The enqueuing handler has already saved the user message and marked the conversation Running.</summary>
public class AIDispatchTurnJob(
    IServiceScopeFactory scopeFactory,
    ILogger<AIDispatchTurnJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(
        Guid tenantId, Guid conversationId, Guid? triggeredByUserId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        // Hangfire bypasses the request pipeline, so [RequiresFeature] is inert here.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenantId, TenantFeature.AIDispatch))
        {
            logger.LogWarning(
                "AIDispatch feature is disabled for tenant {TenantId}; skipping turn for conversation {ConversationId}",
                tenantId, conversationId);
            return;
        }

        var dispatchService = scope.ServiceProvider.GetRequiredService<IAIDispatchService>();

        try
        {
            await dispatchService.RunTurnAsync(
                new AIDispatchTurnRequest(tenantId, conversationId, triggeredByUserId), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dispatch turn failed for conversation {ConversationId} (tenant {TenantId})",
                conversationId, tenantId);
        }
    }
}

public class HangfireAIDispatchTurnRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<AIDispatchTurnRequest>
{
    public void Enqueue(AIDispatchTurnRequest request)
    {
        jobClient.Enqueue<AIDispatchTurnJob>(job => job.RunAsync(
            request.TenantId,
            request.ConversationId,
            request.TriggeredByUserId,
            CancellationToken.None));
    }
}
