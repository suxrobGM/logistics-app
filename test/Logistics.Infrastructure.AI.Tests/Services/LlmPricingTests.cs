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
}
