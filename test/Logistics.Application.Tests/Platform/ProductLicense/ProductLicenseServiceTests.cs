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
    private readonly LicenseKeyFactory _keys = new();
    private readonly ISystemSettingsService _settings = Substitute.For<ISystemSettingsService>();
    private readonly ProductLicenseOptions _options = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ProductLicenseService _sut;

    public ProductLicenseServiceTests()
    {
        _sut = new ProductLicenseService(
            _settings,
            Options.Create(_options),
            new ProductLicenseKeyValidator(_keys.PublicKey),
            _cache);
    }

    public void Dispose()
    {
        _keys.Dispose();
        _cache.Dispose();
    }

    [Fact]
    public async Task GetStatusAsync_NoKeyAnywhere_Unlicensed()
    {
        var status = await _sut.GetStatusAsync();

        Assert.False(status.IsLicensed);
        Assert.Equal(ProductLicenseKeySource.None, status.Source);
        Assert.Equal("no license key", status.Error);
        Assert.Null(status.InstanceId);
    }

    [Fact]
    public async Task GetStatusAsync_ConfigurationKeyWinsOverSystemSettings()
    {
        _options.Key = _keys.Sign(licensee: "Config Co");
        _settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns("garbage");

        var status = await _sut.GetStatusAsync();

        Assert.True(status.IsLicensed);
        Assert.Equal("Config Co", status.Licensee);
        Assert.Equal(ProductLicenseKeySource.Configuration, status.Source);
        await _settings.DidNotReceive().GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatusAsync_StoredKey_ReportsSystemSettingsSource()
    {
        _settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(_keys.Sign());

        var status = await _sut.GetStatusAsync();

        Assert.True(status.IsLicensed);
        Assert.Equal(ProductLicenseKeySource.SystemSettings, status.Source);
    }

    [Fact]
    public async Task GetStatusAsync_SecondCall_ServedFromCache()
    {
        _settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(_keys.Sign());

        await _sut.GetStatusAsync();
        await _sut.GetStatusAsync();

        await _settings.Received(1).GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvalidateCache_ThenGetStatus_ReadsAgain()
    {
        _settings.GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>()).Returns(_keys.Sign());

        await _sut.GetStatusAsync();
        _sut.InvalidateCache();
        await _sut.GetStatusAsync();

        await _settings.Received(2).GetAsync(ProductLicenseSettingsKeys.Key, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateInstanceIdAsync_Missing_PersistsNewGuid()
    {
        _settings.GetAsync(ProductLicenseSettingsKeys.InstanceId, Arg.Any<CancellationToken>()).Returns((string?)null);

        var id = await _sut.GetOrCreateInstanceIdAsync();

        Assert.NotEqual(Guid.Empty, id);
        await _settings.Received(1).SetAsync(
            ProductLicenseSettingsKeys.InstanceId, id.ToString(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateInstanceIdAsync_Existing_ReturnsStoredValue()
    {
        var stored = Guid.NewGuid();
        _settings.GetAsync(ProductLicenseSettingsKeys.InstanceId, Arg.Any<CancellationToken>()).Returns(stored.ToString());

        var id = await _sut.GetOrCreateInstanceIdAsync();

        Assert.Equal(stored, id);
        await _settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
    }
}
