using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class LlmModelResolverTests
{
    private readonly ISystemSettingsService systemSettings = Substitute.For<ISystemSettingsService>();
    private readonly LlmModelResolver sut;

    public LlmModelResolverTests()
    {
        sut = new LlmModelResolver(systemSettings, NullLogger<LlmModelResolver>.Instance);
    }

    private static LlmOptions ConfigWith(params LlmProvider[] providersWithKeys)
    {
        var providers = new Dictionary<LlmProvider, LlmProviderOptions>
        {
            [LlmProvider.Anthropic] = new() { ApiKey = "", Model = "claude-haiku-4-5" },
            [LlmProvider.OpenAi] = new() { ApiKey = "", Model = "gpt-5.4-mini" },
            [LlmProvider.DeepSeek] = new() { ApiKey = "", Model = "deepseek-v4-flash" }
        };

        foreach (var provider in providersWithKeys)
        {
            providers[provider].ApiKey = $"key-{provider}";
        }

        return new LlmOptions
        {
            DefaultProvider = LlmProvider.Anthropic,
            MaxTokens = 4096,
            Providers = providers
        };
    }

    #region Global resolution

    [Fact]
    public async Task ResolveAsync_NoOverride_UsesSystemSetting()
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>())
            .Returns("claude-opus-4-8");

        var selection = await sut.ResolveAsync(ConfigWith(LlmProvider.Anthropic));

        Assert.Equal("claude-opus-4-8", selection.Model);
        Assert.Equal(LlmProvider.Anthropic, selection.Provider);
    }

    [Fact]
    public async Task ResolveAsync_NoSettingAndNoOverride_FallsBackToAppsettings()
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>()).Returns((string?)null);

        var selection = await sut.ResolveAsync(ConfigWith(LlmProvider.Anthropic));

        Assert.Equal("claude-haiku-4-5", selection.Model);
        Assert.Equal(LlmProvider.Anthropic, selection.Provider);
    }

    #endregion

    #region Per-request override

    [Fact]
    public async Task ResolveAsync_KnownOverrideWithApiKey_UsesItAndDerivesProvider()
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>())
            .Returns("claude-opus-4-8");

        var selection = await sut.ResolveAsync(
            ConfigWith(LlmProvider.Anthropic, LlmProvider.DeepSeek), "deepseek-v4-flash");

        Assert.Equal("deepseek-v4-flash", selection.Model);
        // Provider comes from the catalog, so it cannot drift from the model.
        Assert.Equal(LlmProvider.DeepSeek, selection.Provider);
    }

    [Fact]
    public async Task ResolveAsync_UnknownOverride_FallsBackToGlobalModel()
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>())
            .Returns("claude-opus-4-8");

        var selection = await sut.ResolveAsync(
            ConfigWith(LlmProvider.Anthropic), "gpt-9-imaginary");

        Assert.Equal("claude-opus-4-8", selection.Model);
    }

    /// <summary>
    /// A deployment may configure only one provider's key. Honouring an override whose provider has
    /// no key would hard-fail the nightly learning on every such install.
    /// </summary>
    [Fact]
    public async Task ResolveAsync_OverrideProviderHasNoApiKey_FallsBackToGlobalModel()
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>())
            .Returns("claude-opus-4-8");

        var selection = await sut.ResolveAsync(
            ConfigWith(LlmProvider.Anthropic), "deepseek-v4-flash");

        Assert.Equal("claude-opus-4-8", selection.Model);
        Assert.Equal(LlmProvider.Anthropic, selection.Provider);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ResolveAsync_BlankOverride_IsIgnored(string? modelId)
    {
        systemSettings.GetAsync(AiSettingsKeys.Model, Arg.Any<CancellationToken>())
            .Returns("claude-opus-4-8");

        var selection = await sut.ResolveAsync(ConfigWith(LlmProvider.Anthropic), modelId);

        Assert.Equal("claude-opus-4-8", selection.Model);
    }

    #endregion
}
