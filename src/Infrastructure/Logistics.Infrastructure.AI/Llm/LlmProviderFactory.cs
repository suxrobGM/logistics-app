using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// Creates <see cref="ILlmProvider"/> instances based on the configured or requested provider type.
/// Anthropic uses its native SDK; all other providers use the OpenAI-compatible SDK.
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
            _ => new OpenAILlmProvider(config, httpClient)
        };
    }
}
