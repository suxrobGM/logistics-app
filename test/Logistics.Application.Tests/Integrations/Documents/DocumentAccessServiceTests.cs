using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Integrations.Documents.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.Documents;

public class DocumentAccessServiceTests
{
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IUserPermissionService userPermissions = Substitute.For<IUserPermissionService>();
    private readonly DocumentAccessService sut;

    private readonly Guid callerId = Guid.NewGuid();
    private readonly Guid tenantId = Guid.NewGuid();

    public DocumentAccessServiceTests()
    {
        currentUser.GetUserId().Returns(callerId);
        currentUser.GetTenantId().Returns(tenantId);
        sut = new DocumentAccessService(
            Substitute.For<ITenantUnitOfWork>(), currentUser, userPermissions);
    }

    private void CallerHolds(params string[] permissions) =>
        userPermissions.GetPermissionsAsync(callerId, tenantId, Arg.Any<CancellationToken>())
            .Returns(permissions.ToHashSet());

    [Fact]
    public async Task ResolveCaller_CustomRoleWithDocumentView_IsAllowedIn()
    {
        CallerHolds(Permission.Document.View);

        var caller = await sut.ResolveCallerAsync();

        Assert.NotNull(caller);
        Assert.False(caller.IsReviewer);
    }

    [Fact]
    public async Task ResolveCaller_WithReview_IsReviewer()
    {
        CallerHolds(Permission.Document.View, Permission.Document.Review);

        var caller = await sut.ResolveCallerAsync();

        Assert.NotNull(caller);
        Assert.True(caller.IsReviewer);
    }

    [Fact]
    public async Task ResolveCaller_NoDocumentPermission_IsRejected()
    {
        CallerHolds(Permission.Load.View);

        Assert.Null(await sut.ResolveCallerAsync());
    }

    [Fact]
    public async Task ResolveCaller_Unauthenticated_IsRejected()
    {
        currentUser.GetUserId().Returns((Guid?)null);

        Assert.Null(await sut.ResolveCallerAsync());
    }

    [Theory]
    [InlineData(TenantRoles.Owner, true)]
    [InlineData(TenantRoles.Manager, true)]
    [InlineData(TenantRoles.Dispatcher, true)]
    [InlineData(TenantRoles.Driver, false)]
    public void BuiltInRoles_KeepTheAccessTheyHadBeforeThePermissionExisted(
        string role, bool isReviewer)
    {
        var granted = TenantRolePermissions.GetPermissionsForRole(role).ToHashSet();

        Assert.Contains(Permission.Document.View, granted);
        Assert.Contains(Permission.Document.Manage, granted);
        Assert.Equal(isReviewer, granted.Contains(Permission.Document.Review));
    }

    [Fact]
    public void DocumentPermissions_AreDiscoverableByReflection()
    {
        // A non-static nested class or a static readonly field is silently dropped from GetAll,
        // which would leave SuperAdmin without the permission and nobody able to use it.
        var all = Permission.GetAll().ToHashSet();

        Assert.Contains(Permission.Document.View, all);
        Assert.Contains(Permission.Document.Manage, all);
        Assert.Contains(Permission.Document.Review, all);
    }
}
