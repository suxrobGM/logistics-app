using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Modules.IdentityAccess.Tenants.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Tenants;

public class TenantAdminFieldsTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<Tenant, Guid> _tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();
    private readonly Tenant _tenant = TestTenant.Create();

    public TenantAdminFieldsTests()
    {
        _masterUow.Repository<Tenant>().Returns(_tenantRepo);
        _tenantRepo.GetByIdAsync(_tenant.Id, Arg.Any<CancellationToken>()).Returns(_tenant);
    }

    [Fact]
    public async Task UpdateTenant_SettingsWithAIEnabled_KeepsStoredAIEnabled()
    {
        _tenant.Settings = new TenantSettings { AIEnabled = false };
        var sut = new UpdateTenantHandler(_masterUow, Substitute.For<IStripeCustomerService>());

        var result = await sut.Handle(new UpdateTenantCommand
        {
            Id = _tenant.Id,
            Settings = new TenantSettings { AIEnabled = true, Timezone = "Europe/Berlin" }
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(_tenant.Settings.AIEnabled);
        Assert.Equal("Europe/Berlin", _tenant.Settings.Timezone);
    }

    [Fact]
    public async Task UpdateTenantAdminSettings_NameTaken_Fails()
    {
        _tenantRepo.CountAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(1);
        var sut = new UpdateTenantAdminSettingsHandler(_masterUow);

        var result = await sut.Handle(
            new UpdateTenantAdminSettingsCommand { TenantId = _tenant.Id, Name = "Other" },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("test", _tenant.Name);
        await _masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTenantAdminSettings_FreeNameAndAIFlag_UpdatesBoth()
    {
        var sut = new UpdateTenantAdminSettingsHandler(_masterUow);

        var result = await sut.Handle(
            new UpdateTenantAdminSettingsCommand { TenantId = _tenant.Id, Name = " Other ", AIEnabled = false },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("other", _tenant.Name);
        Assert.False(_tenant.Settings.AIEnabled);
    }
}
