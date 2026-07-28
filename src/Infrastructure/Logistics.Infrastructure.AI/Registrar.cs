using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.AIDispatch;

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

        // Provider factory and shared model resolution
        services.AddSingleton<LlmProviderFactory>();
        services.AddScoped<LlmModelResolver>();

        // One-shot LLM client (used by non-agent features, e.g. PDF parsing)
        services.AddScoped<ILlmClient, LlmClient>();

        // Agent services
        services.AddSingleton<AIDispatchSessionCancellationRegistry>();
        services.AddScoped<AgentLoopRunner>();
        services.AddScoped<IAIDispatchService, AIDispatchService>();
        services.AddScoped<AIDispatchConversationBuilder>();
        services.AddScoped<AIDispatchDecisionProcessor>();
        services.AddScoped<IAIDispatchToolExecutor, AIDispatchToolExecutor>();
        services.AddSingleton<IAIDispatchToolRegistry, AIDispatchToolRegistry>();

        // Copilot (conversational agent sharing the loop, tools, and decision machinery)
        services.AddScoped<IAICopilotService, AICopilotService>();
        services.AddScoped<AICopilotConversationBuilder>();

        // Individual dispatch tools
        services.AddScoped<IAIDispatchTool, GetUnassignedLoadsTool>();
        services.AddScoped<IAIDispatchTool, GetAvailableTrucksTool>();
        services.AddScoped<IAIDispatchTool, GetDriverHosTool>();
        services.AddScoped<IAIDispatchTool, CheckHosFeasibilityTool>();
        services.AddScoped<IAIDispatchTool, BatchCheckHosFeasibilityTool>();
        services.AddScoped<IAIDispatchTool, CheckDispatchEligibilityTool>();
        services.AddScoped<IAIDispatchTool, CalculateDistanceTool>();
        services.AddScoped<IAIDispatchTool, OptimizeTripStopsTool>();
        services.AddScoped<IAIDispatchTool, AssignLoadToTruckTool>();
        services.AddScoped<IAIDispatchTool, CreateTripTool>();
        services.AddScoped<IAIDispatchTool, DispatchTripTool>();
        services.AddScoped<IAIDispatchTool, CalculateAssignmentMetricsTool>();
        services.AddScoped<IAIDispatchTool, PreviewTaxCalculationTool>();
        services.AddScoped<IAIDispatchTool, GetContainerStatusTool>();
        services.AddScoped<IAIDispatchTool, GetTerminalInfoTool>();

        // Load board tools (conditionally included in tool definitions based on tenant feature flag)
        services.AddScoped<IAIDispatchTool, SearchLoadBoardTool>();
        services.AddScoped<IAIDispatchTool, CheckBrokerCreditTool>();
        services.AddScoped<IAIDispatchTool, BookLoadBoardLoadTool>();

        // Copilot tools (loads, customers, invoicing, expenses, maintenance)
        services.AddScoped<IAIDispatchTool, SearchLoadsTool>();
        services.AddScoped<IAIDispatchTool, GetLoadTool>();
        services.AddScoped<IAIDispatchTool, SearchCustomersTool>();
        services.AddScoped<IAIDispatchTool, GetInvoicesTool>();
        services.AddScoped<IAIDispatchTool, GetInvoiceTool>();
        services.AddScoped<IAIDispatchTool, SearchExpensesTool>();
        services.AddScoped<IAIDispatchTool, GetExpenseStatsTool>();
        services.AddScoped<IAIDispatchTool, GetUpcomingMaintenanceTool>();
        services.AddScoped<IAIDispatchTool, CreateLoadInvoiceTool>();
        services.AddScoped<IAIDispatchTool, SendInvoiceTool>();
        services.AddScoped<IAIDispatchTool, CreatePaymentLinkTool>();

        return services;
    }
}
