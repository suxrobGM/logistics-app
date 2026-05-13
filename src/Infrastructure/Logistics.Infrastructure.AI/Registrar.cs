using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.AiDispatch;

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
        services.AddSingleton<AiDispatchSessionCancellationRegistry>();
        services.AddScoped<IAiDispatchService, AiDispatchService>();
        services.AddScoped<AiDispatchConversationBuilder>();
        services.AddScoped<AiDispatchDecisionProcessor>();
        services.AddScoped<IAiDispatchToolExecutor, AiDispatchToolExecutor>();
        services.AddSingleton<IAiDispatchToolRegistry, AiDispatchToolRegistry>();

        // Individual dispatch tools
        services.AddScoped<IAiDispatchTool, GetUnassignedLoadsTool>();
        services.AddScoped<IAiDispatchTool, GetAvailableTrucksTool>();
        services.AddScoped<IAiDispatchTool, GetDriverHosTool>();
        services.AddScoped<IAiDispatchTool, CheckHosFeasibilityTool>();
        services.AddScoped<IAiDispatchTool, BatchCheckHosFeasibilityTool>();
        services.AddScoped<IAiDispatchTool, CheckDispatchEligibilityTool>();
        services.AddScoped<IAiDispatchTool, CalculateDistanceTool>();
        services.AddScoped<IAiDispatchTool, OptimizeTripStopsTool>();
        services.AddScoped<IAiDispatchTool, AssignLoadToTruckTool>();
        services.AddScoped<IAiDispatchTool, CreateTripTool>();
        services.AddScoped<IAiDispatchTool, DispatchTripTool>();
        services.AddScoped<IAiDispatchTool, CalculateAssignmentMetricsTool>();
        services.AddScoped<IAiDispatchTool, PreviewTaxCalculationTool>();

        // Load board tools (conditionally included in tool definitions based on tenant feature flag)
        services.AddScoped<IAiDispatchTool, SearchLoadBoardTool>();
        services.AddScoped<IAiDispatchTool, BookLoadBoardLoadTool>();

        return services;
    }
}
