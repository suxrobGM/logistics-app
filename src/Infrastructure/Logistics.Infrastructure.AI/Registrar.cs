using Logistics.Application.Abstractions.Agents;
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
        services.AddSingleton<AgentSessionCancellationRegistry>();
        services.AddScoped<LlmSessionSetup>();
        services.AddScoped<AgentLoopRunner>();
        services.AddScoped<AgentOverageReporter>();
        services.AddScoped<AgentDecisionProcessor>();
        services.AddScoped<IAgentToolExecutor, AgentToolExecutor>();
        services.AddSingleton<IAgentToolRegistry, AgentToolRegistry>();

        services.AddScoped<AgentTurnService>();

        services.AddScoped<IAIDispatchService, AIDispatchService>();
        services.AddScoped<DispatchAgentSurface>();
        services.AddScoped<AIDispatchConversationBuilder>();

        services.AddScoped<IAICopilotService, AICopilotService>();
        services.AddScoped<CopilotAgentSurface>();
        services.AddScoped<AICopilotConversationBuilder>();

        services.AddAgentTools();

        return services;
    }

    /// <summary>
    ///     Registers every tool the catalogue discovered, by its own type: the executor resolves the
    ///     one tool a call names instead of building all of them to find it.
    /// </summary>
    private static void AddAgentTools(this IServiceCollection services)
    {
        foreach (var toolType in AgentToolCatalog.ToolTypes)
        {
            services.AddScoped(toolType);
        }
    }
}
