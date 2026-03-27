using Logistics.Application.Services;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.AI;

public static class Registrar
{
    /// <summary>
    ///     Add AI infrastructure (Claude API dispatch agent).
    /// </summary>
    public static IServiceCollection AddAIInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ClaudeOptions>(configuration.GetSection(ClaudeOptions.SectionName));

        // Agent services
        services.AddScoped<IDispatchAgentService, ClaudeDispatchAgentService>();
        services.AddScoped<DispatchConversationBuilder>();
        services.AddScoped<DispatchDecisionProcessor>();
        services.AddScoped<IDispatchToolExecutor, DispatchToolExecutor>();
        services.AddSingleton<IDispatchToolRegistry, DispatchToolRegistry>();

        // Individual dispatch tools
        services.AddScoped<IDispatchTool, GetUnassignedLoadsTool>();
        services.AddScoped<IDispatchTool, GetAvailableTrucksTool>();
        services.AddScoped<IDispatchTool, GetDriverHosTool>();
        services.AddScoped<IDispatchTool, CheckHosFeasibilityTool>();
        services.AddScoped<IDispatchTool, BatchCheckHosFeasibilityTool>();
        services.AddScoped<IDispatchTool, CalculateDistanceTool>();
        services.AddScoped<IDispatchTool, OptimizeTripStopsTool>();
        services.AddScoped<IDispatchTool, AssignLoadToTruckTool>();
        services.AddScoped<IDispatchTool, CreateTripTool>();
        services.AddScoped<IDispatchTool, DispatchTripTool>();

        // TODO: Load board tools are stubs - register only when integration is configured
        // services.AddScoped<IDispatchTool, SearchLoadBoardTool>();
        // services.AddScoped<IDispatchTool, BookLoadBoardLoadTool>();

        return services;
    }
}
