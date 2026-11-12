using Logistics.Application.Abstractions.Accounting;
using Logistics.Infrastructure.Persistence.Services.Accounting;
using Microsoft.AspNetCore.DataProtection;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services.Accounting;

public class OAuthStateProtectorTests
{
    private readonly IDataProtectionProvider _dataProtection = new EphemeralDataProtectionProvider();
    private readonly TimeProvider _clock = Substitute.For<TimeProvider>();
    private readonly OAuthStateProtector _sut;
    private DateTimeOffset _now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    public OAuthStateProtectorTests()
    {
        _clock.GetUtcNow().Returns(_ => _now);
        _sut = new OAuthStateProtector(_dataProtection, _clock);
    }

    [Fact]
    public void TryUnprotect_FreshState_ReturnsTenantAndUser()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var state = _sut.TryUnprotect(_sut.Protect(tenantId, userId));

        Assert.Equal(new OAuthState(tenantId, userId), state);
    }

    [Fact]
    public void TryUnprotect_AfterLifetime_ReturnsNull()
    {
        var protectedState = _sut.Protect(Guid.NewGuid(), Guid.NewGuid());

        _now = _now.Add(IOAuthStateProtector.Lifetime).AddSeconds(1);

        Assert.Null(_sut.TryUnprotect(protectedState));
    }

    [Fact]
    public void TryUnprotect_JustBeforeExpiry_StillValid()
    {
        var protectedState = _sut.Protect(Guid.NewGuid(), Guid.NewGuid());

        _now = _now.Add(IOAuthStateProtector.Lifetime).AddSeconds(-1);

        Assert.NotNull(_sut.TryUnprotect(protectedState));
    }

    [Fact]
    public void TryUnprotect_TamperedState_ReturnsNull()
    {
        var protectedState = _sut.Protect(Guid.NewGuid(), Guid.NewGuid());
        var tampered = protectedState[..^2] + (protectedState[^1] == 'A' ? "BB" : "AA");

        Assert.Null(_sut.TryUnprotect(tampered));
    }

    [Fact]
    public void TryUnprotect_PlainTenantIdFromAnotherProtector_ReturnsNull()
    {
        var foreign = _dataProtection.CreateProtector("Logistics.OAuthState.v1").Protect(Guid.NewGuid().ToString());

        Assert.Null(_sut.TryUnprotect(foreign));
    }

    [Fact]
    public void Protect_SameTenantAndUser_ProducesDistinctStates()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        Assert.NotEqual(_sut.Protect(tenantId, userId), _sut.Protect(tenantId, userId));
    }
}
