using Logistics.Infrastructure.AI.Tools.Dispatch;
using Logistics.Infrastructure.AI.Tools.Operations;
using Logistics.Infrastructure.AI.Tools.Financial;
using Logistics.Infrastructure.AI.Tools.LoadBoard;
using Logistics.Infrastructure.AI.Tools.Intermodal;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Agents;
using Logistics.Infrastructure.AI.Agents.Copilot;
using Logistics.Infrastructure.AI.Agents.Dispatch;
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
        var llmSection = configuration.GetSection(LlmOptions.SectionName);
        services.Configure<LlmOptions>(llmSection);

        // Both SDKs otherwise run on their own defaults with no ceiling we control. At 25 iterations
        // per session, one wedged connection would tie up a Hangfire worker for a very long time.
        var requestTimeout = llmSection.Get<LlmOptions>()?.RequestTimeoutSeconds
            ?? new LlmOptions().RequestTimeoutSeconds;
        services.AddHttpClient(LlmProviderFactory.HttpClientName,
            client => client.Timeout = TimeSpan.FromSeconds(requestTimeout));

        services.AddSingleton<LlmProviderFactory>();
        services.AddScoped<LlmModelResolver>();

        // One-shot LLM client (used by non-agent features, e.g. PDF parsing)
        services.AddScoped<ILlmClient, LlmClient>();

        services.AddScoped<IAgentRunContext, AgentRunContext>();
        services.AddSingleton<AIDispatchSessionCancellationRegistry>();
        services.AddScoped<LlmSessionSetup>();
        services.AddScoped<AgentLoopRunner>();
        services.AddScoped<IAIDispatchService, AIDispatchService>();
        services.AddScoped<AIDispatchConversationBuilder>();
        services.AddScoped<AIDispatchDecisionProcessor>();
        services.AddScoped<IAIDispatchToolExecutor, AIDispatchToolExecutor>();
        services.AddSingleton<IAgentToolRegistry, AgentToolRegistry>();

        // Copilot (conversational agent sharing the loop, tools, and decision machinery)
        services.AddScoped<IAICopilotService, AICopilotService>();
        services.AddScoped<AICopilotConversationBuilder>();

        services.AddScoped<IAgentTool, GetUnassignedLoadsTool>();
        services.AddScoped<IAgentTool, GetAvailableTrucksTool>();
        services.AddScoped<IAgentTool, GetDriverHosTool>();
        services.AddScoped<IAgentTool, CheckHosFeasibilityTool>();
        services.AddScoped<IAgentTool, BatchCheckHosFeasibilityTool>();
        services.AddScoped<IAgentTool, CheckDispatchEligibilityTool>();
        services.AddScoped<IAgentTool, CalculateDistanceTool>();
        services.AddScoped<IAgentTool, AssignLoadToTruckTool>();
        services.AddScoped<IAgentTool, CreateTripTool>();
        services.AddScoped<IAgentTool, DispatchTripTool>();
        services.AddScoped<IAgentTool, CalculateAssignmentMetricsTool>();
        services.AddScoped<IAgentTool, PreviewTaxCalculationTool>();
        services.AddScoped<IAgentTool, GetContainerStatusTool>();
        services.AddScoped<IAgentTool, GetTerminalInfoTool>();

        // Load board tools (conditionally included in tool definitions based on tenant feature flag)
        services.AddScoped<IAgentTool, SearchLoadBoardTool>();
        services.AddScoped<IAgentTool, CheckBrokerCreditTool>();
        services.AddScoped<IAgentTool, BookLoadBoardLoadTool>();

        // Copilot tools (loads, customers, invoicing, expenses, maintenance)
        services.AddScoped<IAgentTool, SearchLoadsTool>();
        services.AddScoped<IAgentTool, GetLoadTool>();
        services.AddScoped<IAgentTool, SearchCustomersTool>();
        services.AddScoped<IAgentTool, GetInvoicesTool>();
        services.AddScoped<IAgentTool, GetInvoiceTool>();
        services.AddScoped<IAgentTool, SearchExpensesTool>();
        services.AddScoped<IAgentTool, GetExpenseStatsTool>();
        services.AddScoped<IAgentTool, GetUpcomingMaintenanceTool>();
        services.AddScoped<IAgentTool, CreateLoadInvoiceTool>();
        services.AddScoped<IAgentTool, SendInvoiceTool>();
        services.AddScoped<IAgentTool, CreatePaymentLinkTool>();

        return services;
    }
}
