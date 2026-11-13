using Logistics.Application.Abstractions.AI;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm;

public class LlmModelCatalogTests
{
    public static TheoryData<string, int, int, decimal> CalculateCost_BaseTokenPricing_ReturnsCorrectCostCases => new()
    {
        { "claude-sonnet-5-5", 1_000_000, 0, 2.0m },
        { "claude-sonnet-5-5", 0, 1_000_000, 10.0m },
        { "claude-sonnet-5-5", 1000, 0, 0.002m },
        { "claude-sonnet-5-5", 0, 0, 0m },
        { "claude-haiku-5-5", 1_000_000, 0, 0.10m },
        { "claude-haiku-5-5", 0, 1_000_000, 0.50m },
        { "gpt-6.1-sol", 1_000_000, 0, 2.0m },
        { "gpt-6.1-sol", 0, 1_000_000, 10.0m },
        { "gpt-6-luna", 1_000_000, 0, 0.10m },
        { "gpt-6-luna", 0, 1_000_000, 0.50m },
        { "deepseek-flash", 1_000_000, 0, 0.30m },
        { "deepseek-flash", 0, 1_000_000, 1.20m },
    };

    [Theory]
    [MemberData(nameof(CalculateCost_BaseTokenPricing_ReturnsCorrectCostCases))]
    public void CalculateCost_BaseTokenPricing_ReturnsCorrectCost(string model, int input, int output, decimal expected)
    {
        var result = LlmModelCatalog.CalculateCost(model, input, output);

        Assert.Equal(expected, result);
    }

    public static TheoryData<string, decimal, decimal> CalculateCost_CacheTokens_IncludedInCostCases => new()
    {
        { "claude-sonnet-5-5", 0.20m, 2.50m },
        { "gpt-6-luna", 0.01m, 0.125m },
    };

    [Theory]
    [MemberData(nameof(CalculateCost_CacheTokens_IncludedInCostCases))]
    public void CalculateCost_CacheTokens_IncludedInCost(string model, decimal cacheRead, decimal cacheWrite)
    {
        var result = LlmModelCatalog.CalculateCost(
            model, 0, 0, cacheReadTokens: 1_000_000, cacheCreationTokens: 1_000_000);

        Assert.Equal(cacheRead + cacheWrite, result);
    }

    [Fact]
    public void CalculateCost_CachedInput_CostsLessThanUncached()
    {
        // Catches the OpenAI cache overcharge: cached tokens sit inside input_tokens, so a
        // provider that forwards both raw prices a cache hit at full rate or higher.
        var cached = LlmModelCatalog.CalculateCost("gpt-6-luna", 200, 0, cacheReadTokens: 800);
        var uncached = LlmModelCatalog.CalculateCost("gpt-6-luna", 1000, 0);

        Assert.True(cached < uncached, $"cached {cached} should undercut uncached {uncached}");
    }

    [Fact]
    public void CalculateCost_UnknownModel_UsesSonnetDefaults()
    {
        var unknown = LlmModelCatalog.CalculateCost("unknown-model", 1_000_000, 0);
        var sonnet = LlmModelCatalog.CalculateCost("claude-sonnet-5-5", 1_000_000, 0);

        Assert.Equal(sonnet, unknown);
    }
}
