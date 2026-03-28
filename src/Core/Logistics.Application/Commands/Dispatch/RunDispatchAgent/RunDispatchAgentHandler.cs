using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RunDispatchAgentHandler(
    ICurrentUserService currentUser,
    ITenantUnitOfWork tenantUow,
    IServiceScopeFactory scopeFactory,
    ILogger<RunDispatchAgentHandler> logger) : IAppRequestHandler<RunDispatchAgentCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(RunDispatchAgentCommand request, CancellationToken ct)
    {
        // Capture tenant context before spawning background work
        var tenant = tenantUow.GetCurrentTenant();
        var agentRequest = new DispatchAgentRequest(
            TenantId: tenant.Id,
            Mode: request.Mode,
            TriggeredByUserId: currentUser.GetUserId(),
            Instructions: request.Instructions);

        // Fire-and-forget: run the agent in a background scope so the HTTP response returns immediately
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var backgroundUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            backgroundUow.SetCurrentTenant(tenant);

            var agentService = scope.ServiceProvider.GetRequiredService<IDispatchAgentService>();
            try
            {
                await agentService.RunAsync(agentRequest, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background dispatch agent failed for tenant {TenantId}", tenant.Id);
            }
        }, CancellationToken.None);

        return Task.FromResult(Result<Guid>.Ok(Guid.Empty));
    }
}
