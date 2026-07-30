using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm;

public class LlmPricingTests
{
    [Theory]
    [InlineData("claude-sonnet-5", 1_000_000, 0, 0, 0, 2.0)]
    [InlineData("claude-sonnet-5", 0, 1_000_000, 0, 0, 10.0)]
    [InlineData("claude-haiku-4-5", 1_000_000, 0, 0, 0, 1.0)]
    [InlineData("claude-haiku-4-5", 0, 1_000_000, 0, 0, 5.0)]
    [InlineData("gpt-5.6-terra", 1_000_000, 0, 0, 0, 2.0)]
    [InlineData("gpt-5.6-terra", 0, 1_000_000, 0, 0, 12.0)]
    [InlineData("gpt-5.6-luna", 1_000_000, 0, 0, 0, 0.20)]
    [InlineData("gpt-5.6-luna", 0, 1_000_000, 0, 0, 1.20)]
    public void Calculate_BaseTokenPricing_ReturnsCorrectCost(
        string model, int input, int output, int cacheRead, int cacheWrite, decimal expected)
    {
        var result = LlmPricing.Calculate(model, input, output, cacheRead, cacheWrite);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("claude-sonnet-5", 0.20, 2.50)]
    [InlineData("gpt-5.6-luna", 0.02, 0.25)]
    public void Calculate_CacheTokens_IncludedInCost(string model, decimal cacheRead, decimal cacheWrite)
    {
        var result = LlmPricing.Calculate(
            model, 0, 0, cacheReadTokens: 1_000_000, cacheCreationTokens: 1_000_000);

        Assert.Equal(cacheRead + cacheWrite, result);
    }

    [Fact]
    public void Calculate_UnknownModel_UsesSonnetDefaults()
    {
        var unknown = LlmPricing.Calculate("unknown-model", 1_000_000, 0);
        var sonnet = LlmPricing.Calculate("claude-sonnet-5", 1_000_000, 0);

        Assert.Equal(sonnet, unknown);
    }

    [Fact]
    public void Calculate_ZeroTokens_ReturnsZero()
    {
        var result = LlmPricing.Calculate("claude-sonnet-5", 0, 0);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void Calculate_SmallTokenCount_ReturnsSubDollarAmount()
    {
        // 1000 input tokens of Sonnet 5 = $2 / 1000 = $0.002
        var result = LlmPricing.Calculate("claude-sonnet-5", 1000, 0);

        Assert.Equal(0.002m, result);
    }

    #region GetMultiplier

    [Theory]
    [InlineData("deepseek-v4-flash", 1)]
    [InlineData("deepseek-v4-pro", 1)]
    [InlineData("gpt-5.6-luna", 1)]
    [InlineData("claude-haiku-4-5", 1)]
    [InlineData("gpt-5.6-terra", 2)]
    [InlineData("claude-sonnet-5", 2)]
    [InlineData("unknown-model", 1)]
    public void GetMultiplier_ReturnsCorrectValue(string model, int expected)
    {
        Assert.Equal(expected, LlmPricing.GetMultiplier(model));
    }

    #endregion

    #region GetOverageBillingUnits

    [Theory]
    [InlineData("deepseek-v4-flash", 1)]
    [InlineData("claude-haiku-4-5", 1)]
    [InlineData("gpt-5.6-luna", 1)]
    [InlineData("gpt-5.6-terra", 2)]
    [InlineData("claude-sonnet-5", 2)]
    [InlineData("unknown-model", 1)]
    public void GetOverageBillingUnits_ReturnsCorrectUnits(string model, int expected)
    {
        Assert.Equal(expected, LlmPricing.GetOverageBillingUnits(model));
    }

    #endregion

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
