using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Services;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class LlmPricingTests
{
    [Theory]
    [InlineData("claude-sonnet-4-6", 1_000_000, 0, 0, 0, 3.0)]
    [InlineData("claude-sonnet-4-6", 0, 1_000_000, 0, 0, 15.0)]
    [InlineData("claude-haiku-4-5", 1_000_000, 0, 0, 0, 1.0)]
    [InlineData("claude-haiku-4-5", 0, 1_000_000, 0, 0, 5.0)]
    [InlineData("claude-opus-4-6", 1_000_000, 0, 0, 0, 5.0)]
    [InlineData("claude-opus-4-6", 0, 1_000_000, 0, 0, 25.0)]
    public void Calculate_BaseTokenPricing_ReturnsCorrectCost(
        string model, int input, int output, int cacheRead, int cacheWrite, decimal expected)
    {
        var result = LlmPricing.Calculate(model, input, output, cacheRead, cacheWrite);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_CacheTokens_IncludedInCost()
    {
        // Sonnet: cache read = $0.30/MTok, cache write = $3.75/MTok
        var result = LlmPricing.Calculate(
            "claude-sonnet-4-6", 0, 0, cacheReadTokens: 1_000_000, cacheCreationTokens: 1_000_000);

        Assert.Equal(0.30m + 3.75m, result);
    }

    [Fact]
    public void Calculate_UnknownModel_UsesSonnetDefaults()
    {
        var unknown = LlmPricing.Calculate("unknown-model", 1_000_000, 0);
        var sonnet = LlmPricing.Calculate("claude-sonnet-4-6", 1_000_000, 0);

        Assert.Equal(sonnet, unknown);
    }

    [Fact]
    public void Calculate_ZeroTokens_ReturnsZero()
    {
        var result = LlmPricing.Calculate("claude-sonnet-4-6", 0, 0);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void Calculate_SmallTokenCount_ReturnsSubDollarAmount()
    {
        // 1000 input tokens of Sonnet = $3 / 1000 = $0.003
        var result = LlmPricing.Calculate("claude-sonnet-4-6", 1000, 0);

        Assert.Equal(0.003m, result);
    }

    #region GetMultiplier

    [Theory]
    [InlineData("deepseek-chat", 1)]
    [InlineData("deepseek-reasoner", 1)]
    [InlineData("gpt-5.4-mini", 1)]
    [InlineData("claude-haiku-4-5", 1)]
    [InlineData("gpt-5.4", 5)]
    [InlineData("claude-sonnet-4-6", 5)]
    [InlineData("claude-opus-4-6", 10)]
    [InlineData("unknown-model", 1)]
    public void GetMultiplier_ReturnsCorrectValue(string model, int expected)
    {
        Assert.Equal(expected, LlmPricing.GetMultiplier(model));
    }

    #endregion

    #region GetModelTier

    [Theory]
    [InlineData("deepseek-chat", LlmModelTier.Base)]
    [InlineData("deepseek-reasoner", LlmModelTier.Base)]
    [InlineData("gpt-5.4-mini", LlmModelTier.Base)]
    [InlineData("claude-haiku-4-5", LlmModelTier.Base)]
    [InlineData("gpt-5.4", LlmModelTier.Premium)]
    [InlineData("claude-sonnet-4-6", LlmModelTier.Premium)]
    [InlineData("claude-opus-4-6", LlmModelTier.Ultra)]
    [InlineData("unknown-model", LlmModelTier.Base)]
    public void GetModelTier_ReturnsCorrectTier(string model, LlmModelTier expected)
    {
        Assert.Equal(expected, LlmPricing.GetModelTier(model));
    }

    #endregion

    #region GetOverageBillingUnits

    [Theory]
    [InlineData("deepseek-chat", 1)]
    [InlineData("claude-haiku-4-5", 1)]
    [InlineData("gpt-5.4-mini", 1)]
    [InlineData("gpt-5.4", 2)]
    [InlineData("claude-sonnet-4-6", 2)]
    [InlineData("claude-opus-4-6", 4)]
    [InlineData("unknown-model", 1)]
    public void GetOverageBillingUnits_ReturnsCorrectUnits(string model, int expected)
    {
        Assert.Equal(expected, LlmPricing.GetOverageBillingUnits(model));
    }

    #endregion
}
