using Logistics.Infrastructure.AI.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm;

public class LlmHttpClientTests
{
    /// <summary>
    ///     The host puts the standard resilience handler on every client. Its 10s attempt and 30s
    ///     total timeouts failed every agent turn whose LLM call ran longer than that.
    /// </summary>
    [Fact]
    public void LlmClient_DoesNotInheritHostResilienceHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());
        services.AddAIInfrastructure(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

        Assert.False(HasResilienceHandler(factory.CreateHandler(LlmProviderFactory.HttpClientName)));
        Assert.True(HasResilienceHandler(factory.CreateHandler("any-other-client")));
    }

    private static bool HasResilienceHandler(HttpMessageHandler? handler)
    {
        while (handler is DelegatingHandler delegating)
        {
            if (delegating is ResilienceHandler)
            {
                return true;
            }

            handler = delegating.InnerHandler;
        }

        return false;
    }
}
