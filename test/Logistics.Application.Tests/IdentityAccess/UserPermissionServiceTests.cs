using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess;

public class UserPermissionServiceTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly UserPermissionService _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    public UserPermissionServiceTests()
    {
        _sut = new UserPermissionService(_mediator, _cache);
    }

    private void SetPermissions(params string[] permissions) =>
        _mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Ok(permissions));

    [Fact]
    public async Task GetPermissionsAsync_CalledTwice_ResolvesOnlyOnce()
    {
        SetPermissions("Permission.Dispatch.Manage");

        await _sut.GetPermissionsAsync(_userId, _tenantId);
        var second = await _sut.GetPermissionsAsync(_userId, _tenantId);

        Assert.Contains("Permission.Dispatch.Manage", second);
        await _mediator.Received(1).Send(
            Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>());
    }

    /// <summary>One user's cached permissions must never satisfy another tenant's check.</summary>
    [Fact]
    public async Task GetPermissionsAsync_DifferentTenant_ResolvesSeparately()
    {
        SetPermissions("Permission.Dispatch.Manage");

        await _sut.GetPermissionsAsync(_userId, _tenantId);
        await _sut.GetPermissionsAsync(_userId, Guid.NewGuid());

        await _mediator.Received(2).Send(
            Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A transient failure must not be cached, or one bad lookup locks the user out of every
    /// permission-gated action until the entry expires.
    /// </summary>
    [Fact]
    public async Task GetPermissionsAsync_LookupFails_NotCachedAndRetriedNextTime()
    {
        _mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Fail("database unavailable"));

        Assert.Empty(await _sut.GetPermissionsAsync(_userId, _tenantId));

        SetPermissions("Permission.Dispatch.Manage");

        Assert.Contains("Permission.Dispatch.Manage", await _sut.GetPermissionsAsync(_userId, _tenantId));
    }

    [Fact]
    public async Task HasPermissionAsync_ReflectsTheResolvedSet()
    {
        SetPermissions("Permission.Dispatch.Manage");

        Assert.True(await _sut.HasPermissionAsync(_userId, _tenantId, "Permission.Dispatch.Manage"));
        Assert.False(await _sut.HasPermissionAsync(_userId, _tenantId, "Permission.Invoice.Manage"));
    }
}
