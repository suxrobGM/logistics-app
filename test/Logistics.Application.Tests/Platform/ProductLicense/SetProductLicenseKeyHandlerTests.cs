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
    private readonly LicenseKeyFactory keys = new();
    private readonly ISystemSettingsService settings = Substitute.For<ISystemSettingsService>();
    private readonly IProductLicenseService license = Substitute.For<IProductLicenseService>();
    private readonly ProductLicenseOptions options = new();
    private readonly SetProductLicenseKeyHandler sut;

    public SetProductLicenseKeyHandlerTests()
    {
        license.GetStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new ProductLicenseStatusDto { IsLicensed = true });

        sut = new SetProductLicenseKeyHandler(
            settings,
            license,
            new ProductLicenseKeyValidator(keys.PublicKey),
            Options.Create(options));
    }

    public void Dispose() => keys.Dispose();

    [Fact]
    public async Task Handle_KeyManagedByConfiguration_Fails()
    {
        options.Key = keys.Sign();

        var result = await sut.Handle(new SetProductLicenseKeyCommand { Key = keys.Sign() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("License__Key", result.Error);
        await settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_InvalidKey_FailsAndDoesNotPersist()
    {
        var result = await sut.Handle(new SetProductLicenseKeyCommand { Key = "nonsense" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("malformed key", result.Error);
        await settings.DidNotReceiveWithAnyArgs().SetAsync(default!, default!, default, default);
        license.DidNotReceive().InvalidateCache();
    }

    [Fact]
    public async Task Handle_ExpiredKey_FailsWithReason()
    {
        var key = keys.Sign(DateTime.UtcNow.AddDays(-2));

        var result = await sut.Handle(new SetProductLicenseKeyCommand { Key = key }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("expired", result.Error);
    }

    [Fact]
    public async Task Handle_ValidKey_PersistsAndInvalidatesCache()
    {
        var key = keys.Sign();

        var result = await sut.Handle(new SetProductLicenseKeyCommand { Key = $"  {key}\n" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsLicensed);
        await settings.Received(1).SetAsync(
            ProductLicenseSettingsKeys.Key, key, Arg.Any<string?>(), Arg.Any<CancellationToken>());
        license.Received(1).InvalidateCache();
    }
}
