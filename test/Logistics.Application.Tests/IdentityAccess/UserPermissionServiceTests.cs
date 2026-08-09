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
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
    private readonly UserPermissionService sut;

    private readonly Guid userId = Guid.NewGuid();
    private readonly Guid tenantId = Guid.NewGuid();

    public UserPermissionServiceTests()
    {
        sut = new UserPermissionService(mediator, cache);
    }

    private void SetPermissions(params string[] permissions) =>
        mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Ok(permissions));

    [Fact]
    public async Task GetPermissionsAsync_CalledTwice_ResolvesOnlyOnce()
    {
        SetPermissions("Permission.Dispatch.Manage");

        await sut.GetPermissionsAsync(userId, tenantId);
        var second = await sut.GetPermissionsAsync(userId, tenantId);

        Assert.Contains("Permission.Dispatch.Manage", second);
        await mediator.Received(1).Send(
            Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>());
    }

    /// <summary>One user's cached permissions must never satisfy another tenant's check.</summary>
    [Fact]
    public async Task GetPermissionsAsync_DifferentTenant_ResolvesSeparately()
    {
        SetPermissions("Permission.Dispatch.Manage");

        await sut.GetPermissionsAsync(userId, tenantId);
        await sut.GetPermissionsAsync(userId, Guid.NewGuid());

        await mediator.Received(2).Send(
            Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A transient failure must not be cached, or one bad lookup locks the user out of every
    /// permission-gated action until the entry expires.
    /// </summary>
    [Fact]
    public async Task GetPermissionsAsync_LookupFails_NotCachedAndRetriedNextTime()
    {
        mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Fail("database unavailable"));

        Assert.Empty(await sut.GetPermissionsAsync(userId, tenantId));

        SetPermissions("Permission.Dispatch.Manage");

        Assert.Contains("Permission.Dispatch.Manage", await sut.GetPermissionsAsync(userId, tenantId));
    }

    [Fact]
    public async Task HasPermissionAsync_ReflectsTheResolvedSet()
    {
        SetPermissions("Permission.Dispatch.Manage");

        Assert.True(await sut.HasPermissionAsync(userId, tenantId, "Permission.Dispatch.Manage"));
        Assert.False(await sut.HasPermissionAsync(userId, tenantId, "Permission.Invoice.Manage"));
    }
}
