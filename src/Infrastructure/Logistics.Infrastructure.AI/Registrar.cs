using Logistics.Application.Services;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.AI;

public static class Registrar
{
    /// <summary>
    ///     Add AI infrastructure, including LLM services, agent orchestration, and tools.
    /// </summary>
    public static IServiceCollection AddAIInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));

        // Provider factory
        services.AddSingleton<LlmProviderFactory>();

        // Agent services
        services.AddSingleton<DispatchSessionCancellationRegistry>();
        services.AddScoped<IDispatchAgentService, DispatchAgentService>();
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
        services.AddScoped<IDispatchTool, CalculateAssignmentMetricsTool>();

        // Load board tools (conditionally included in tool definitions based on tenant feature flag)
        services.AddScoped<IDispatchTool, SearchLoadBoardTool>();
        services.AddScoped<IDispatchTool, BookLoadBoardLoadTool>();

        return services;
    }
}
