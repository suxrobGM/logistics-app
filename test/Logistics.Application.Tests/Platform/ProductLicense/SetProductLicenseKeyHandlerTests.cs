using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Modules.Platform.ProductLicense.Commands;
using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Logistics.Domain.Options;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform.ProductLicense;

public class SetProductLicenseKeyHandlerTests : IDisposable
{
    private readonly LicenseKeyFactory _keys = new();
    private readonly ISystemSettingsService _settings = Substitute.For<ISystemSettingsService>();
    private readonly IProductLicenseService _license = Substitute.For<IProductLicenseService>();
    private readonly ProductLicenseOptions _options = new();
    private readonly SetProductLicenseKeyHandler _sut;

    public SetProductLicenseKeyHandlerTests()
    {
        _license.GetStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new ProductLicenseStatusDto { IsLicensed = true });

        _sut = new SetProductLicenseKeyHandler(
            _settings,
            _license,
            new ProductLicenseKeyValidator(_keys.PublicKey),
            Options.Create(_options));
    }

    public void Dispose() => _keys.Dispose();

    [Fact]
    public async Task Handle_KeyManagedByConfiguration_Fails()
    {
        _options.Key = _keys.Sign();

        var result = await _sut.Handle(new SetProductLicenseKeyCommand { Key = _keys.Sign() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("License__Key", result.Error);
        await _settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_InvalidKey_FailsAndDoesNotPersist()
    {
        var result = await _sut.Handle(new SetProductLicenseKeyCommand { Key = "nonsense" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("malformed key", result.Error);
        await _settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
        _license.DidNotReceive().InvalidateCache();
    }

    [Fact]
    public async Task Handle_ExpiredKey_FailsWithReason()
    {
        var key = _keys.Sign(DateTime.UtcNow.AddDays(-2));

        var result = await _sut.Handle(new SetProductLicenseKeyCommand { Key = key }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error);
    }

    [Fact]
    public async Task Handle_ValidKey_PersistsAndInvalidatesCache()
    {
        var key = _keys.Sign();

        var result = await _sut.Handle(new SetProductLicenseKeyCommand { Key = $"  {key}\n" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsLicensed);
        await _settings.Received(1).SetAsync(
            ProductLicenseSettingsKeys.Key, key, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _license.Received(1).InvalidateCache();
    }
}
