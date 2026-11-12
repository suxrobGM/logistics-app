using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm;

public class LlmPricingTests
{
    public static TheoryData<string, int, int, int, int, decimal> Calculate_BaseTokenPricing_ReturnsCorrectCostCases => new()
    {
        { "claude-sonnet-5-5", 1_000_000, 0, 0, 0, 2.0m },
        { "claude-sonnet-5-5", 0, 1_000_000, 0, 0, 10.0m },
        { "claude-haiku-5-5", 1_000_000, 0, 0, 0, 0.10m },
        { "claude-haiku-5-5", 0, 1_000_000, 0, 0, 0.50m },
        { "gpt-6.1-sol", 1_000_000, 0, 0, 0, 2.0m },
        { "gpt-6.1-sol", 0, 1_000_000, 0, 0, 10.0m },
        { "gpt-6-luna", 1_000_000, 0, 0, 0, 0.10m },
        { "gpt-6-luna", 0, 1_000_000, 0, 0, 0.50m },
        { "deepseek-flash", 1_000_000, 0, 0, 0, 0.30m },
        { "deepseek-flash", 0, 1_000_000, 0, 0, 1.20m },
    };

    [Theory]
    [MemberData(nameof(Calculate_BaseTokenPricing_ReturnsCorrectCostCases))]
    public void Calculate_BaseTokenPricing_ReturnsCorrectCost(
        string model, int input, int output, int cacheRead, int cacheWrite, decimal expected)
    {
        var result = LlmPricing.Calculate(model, input, output, cacheRead, cacheWrite);

        Assert.Equal(expected, result);
    }

    public static TheoryData<string, decimal, decimal> Calculate_CacheTokens_IncludedInCostCases => new()
    {
        { "claude-sonnet-5-5", 0.20m, 2.50m },
        { "gpt-6-luna", 0.01m, 0.125m },
    };

    [Theory]
    [MemberData(nameof(Calculate_CacheTokens_IncludedInCostCases))]
    public void Calculate_CacheTokens_IncludedInCost(string model, decimal cacheRead, decimal cacheWrite)
    {
        var result = LlmPricing.Calculate(
            model, 0, 0, cacheReadTokens: 1_000_000, cacheCreationTokens: 1_000_000);

        Assert.Equal(cacheRead + cacheWrite, result);
    }

    [Fact]
    public void Calculate_CachedInput_CostsLessThanUncached()
    {
        // Catches the OpenAI cache overcharge: cached tokens sit inside input_tokens, so a
        // provider that forwards both raw prices a cache hit at full rate or higher.
        var cached = LlmPricing.Calculate("gpt-6-luna", 200, 0, cacheReadTokens: 800);
        var uncached = LlmPricing.Calculate("gpt-6-luna", 1000, 0);

        Assert.True(cached < uncached, $"cached {cached} should undercut uncached {uncached}");
    }

    [Fact]
    public void Calculate_UnknownModel_UsesSonnetDefaults()
    {
        var unknown = LlmPricing.Calculate("unknown-model", 1_000_000, 0);
        var sonnet = LlmPricing.Calculate("claude-sonnet-5-5", 1_000_000, 0);

        Assert.Equal(sonnet, unknown);
    }

    [Fact]
    public void Calculate_ZeroTokens_ReturnsZero()
    {
        var result = LlmPricing.Calculate("claude-sonnet-5-5", 0, 0);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void Calculate_SmallTokenCount_ReturnsSubDollarAmount()
    {
        // 1000 input tokens of Sonnet 5.5 = $2 / 1000 = $0.002
        var result = LlmPricing.Calculate("claude-sonnet-5-5", 1000, 0);

        Assert.Equal(0.002m, result);
    }

    #region Catalog parity

    [Fact]
    public void Pricing_CoversEveryCatalogModel()
    {
        // A catalog model missing from pricing silently falls back to defaults - keep the lists in sync.
        var unpriced = LlmModelCatalog.Models
            .Select(m => m.Id)
            .Where(id => !LlmPricing.KnownModels.Contains(id))
            .ToList();

        Assert.Empty(unpriced);
    }

    [Fact]
    public void Catalog_CoversEveryPricedModel()
    {
        var unlisted = LlmPricing.KnownModels
            .Where(id => !LlmModelCatalog.IsValid(id))
            .ToList();

        Assert.Empty(unlisted);
    }

    #endregion
}
