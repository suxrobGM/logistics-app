using Hangfire;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Runs a single copilot turn. Enqueued from the send-message handler, which has already
///     created the user message and marked the conversation Running.
/// </summary>
public class AICopilotTurnJob(
    IServiceScopeFactory scopeFactory,
    ILogger<AICopilotTurnJob> logger)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task RunAsync(
        Guid tenantId, Guid conversationId, Guid userId, string? pageContext, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        // Hangfire bypasses the MediatR pipeline, so [RequiresFeature] is inert here.
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
            await copilotService.RunTurnAsync(
                new AICopilotTurnRequest(tenantId, conversationId, userId, pageContext), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Copilot turn failed for conversation {ConversationId} (tenant {TenantId})",
                conversationId, tenantId);
        }
    }
}

public class HangfireAICopilotTurnRunner(IBackgroundJobClient jobClient) : IBackgroundJobRunner<AICopilotTurnRequest>
{
    public void Enqueue(AICopilotTurnRequest request)
    {
        jobClient.Enqueue<AICopilotTurnJob>(job => job.RunAsync(
            request.TenantId,
            request.ConversationId,
            request.UserId,
            request.PageContext,
            CancellationToken.None));
    }
}
