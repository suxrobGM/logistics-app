using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Logistics.Domain.Options;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform.ProductLicense;

public class ProductLicenseServiceTests : IDisposable
{
    private readonly LicenseKeyFactory keys = new();
    private readonly ISystemSettingsService settings = Substitute.For<ISystemSettingsService>();
    private readonly ProductLicenseOptions options = new();
    private readonly MemoryCache cache = new(new MemoryCacheOptions());
    private readonly ProductLicenseService sut;

    public ProductLicenseServiceTests()
    {
        sut = new ProductLicenseService(
            settings,
            Options.Create(options),
            new ProductLicenseKeyValidator(keys.PublicKey),
            cache);
    }

    public void Dispose()
    {
        keys.Dispose();
        cache.Dispose();
    }

    [Fact]
    public async Task GetStatusAsync_NoKeyAnywhere_Unlicensed()
    {
        var status = await sut.GetStatusAsync();

        Assert.False(status.IsLicensed);
        Assert.Equal(ProductLicenseKeySource.None, status.Source);
        Assert.Equal("no license key", status.Error);
        Assert.Null(status.InstanceId);
    }

    [Fact]
    public async Task GetStatusAsync_ConfigurationKeyWinsOverSystemSettings()
    {
        options.Key = keys.Sign(licensee: "Config Co");
        settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns("garbage");

        var status = await sut.GetStatusAsync();

        Assert.True(status.IsLicensed);
        Assert.Equal("Config Co", status.Licensee);
        Assert.Equal(ProductLicenseKeySource.Configuration, status.Source);
        await settings.DidNotReceive().GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatusAsync_StoredKey_ReportsSystemSettingsSource()
    {
        settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(keys.Sign());

        var status = await sut.GetStatusAsync();

        Assert.True(status.IsLicensed);
        Assert.Equal(ProductLicenseKeySource.SystemSettings, status.Source);
    }

    [Fact]
    public async Task GetStatusAsync_SecondCall_ServedFromCache()
    {
        settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(keys.Sign());

        await sut.GetStatusAsync();
        await sut.GetStatusAsync();

        await settings.Received(1).GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvalidateCache_ThenGetStatus_ReadsAgain()
    {
        settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(keys.Sign());

        await sut.GetStatusAsync();
        sut.InvalidateCache();
        await sut.GetStatusAsync();

        await settings.Received(2).GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateInstanceIdAsync_Missing_PersistsNewGuid()
    {
        settings.GetAsync(ProductLicenseSettingsKeys.InstanceId, Arg.Any<CancellationToken>()).Returns((string?)null);

        var id = await sut.GetOrCreateInstanceIdAsync();

        Assert.NotEqual(Guid.Empty, id);
        await settings.Received(1).SetAsync(
            ProductLicenseSettingsKeys.InstanceId, id.ToString(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateInstanceIdAsync_Existing_ReturnsStoredValue()
    {
        var stored = Guid.NewGuid();
        settings.GetAsync(ProductLicenseSettingsKeys.InstanceId, Arg.Any<CancellationToken>()).Returns(stored.ToString());

        var id = await sut.GetOrCreateInstanceIdAsync();

        Assert.Equal(stored, id);
        await settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
    }
}
