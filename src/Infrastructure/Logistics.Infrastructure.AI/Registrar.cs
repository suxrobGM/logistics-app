using Logistics.Application.Services;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Services;
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
        services.AddHttpClient<IDispatchAgentService, ClaudeDispatchAgentService>();
        services.AddScoped<IDispatchToolExecutor, DispatchToolExecutor>();
        services.AddSingleton<IDispatchToolRegistry, DispatchToolRegistry>();
        return services;
    }
}
