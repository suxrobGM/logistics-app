using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm.Providers;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Creates <see cref="ILlmProvider"/> instances based on the configured or requested provider type.
/// Routing is on the enum, never the model - the wire protocol belongs to the endpoint.
/// </summary>
internal sealed class LlmProviderFactory(IOptions<LlmOptions> options, IHttpClientFactory httpClientFactory)
{
    /// <summary>
    /// Named client carrying the configured request timeout and pooled connections. Registered in
    /// the AI registrar.
    /// </summary>
    public const string HttpClientName = "llm";

    public ILlmProvider Create(LlmProvider? providerOverride = null)
    {
        var type = providerOverride ?? options.Value.DefaultProvider;
        var config = options.Value.GetProviderConfig(type);
        var httpClient = httpClientFactory.CreateClient(HttpClientName);

        return type switch
        {
            LlmProvider.Anthropic => new AnthropicLlmProvider(config, httpClient),
            LlmProvider.OpenAI => new OpenAIResponsesLlmProvider(config, httpClient),
            _ => new OpenAICompatibleLlmProvider(config, httpClient)
        };
    }
}
