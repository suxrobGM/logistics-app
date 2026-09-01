using System.Reflection;
using Logistics.API.Controllers;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Logistics.Architecture.Tests;

/// <summary>
/// Endpoints whose authorization cannot be covered by a handler test, because the handler is never
/// reached when the attribute is right and the attribute is what a refactor drops.
/// </summary>
public class ControllerAuthorizationTests
{
    private static string[] RolesOn(Type controller, string action)
    {
        var attribute = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>();

        return attribute?.Roles?.Split(',', StringSplitOptions.TrimEntries) ?? [];
    }

    /// <summary>
    /// A bare [Authorize] here is not enough: the API installs a global AuthorizeFilter that already
    /// requires an authenticated user, so every tenant role - Driver and Customer included - would
    /// be able to cancel a paid subscription.
    /// </summary>
    [Theory]
    [InlineData(nameof(SubscriptionController.CancelSubscription))]
    [InlineData(nameof(SubscriptionController.ChangeSubscriptionPlan))]
    [InlineData(nameof(SubscriptionController.RenewSubscription))]
    public void Billing_mutations_are_limited_to_billing_roles(string action)
    {
        var roles = RolesOn(typeof(SubscriptionController), action);

        Assert.Equal(
            [AppRoles.SuperAdmin, AppRoles.Admin, TenantRoles.Owner, TenantRoles.Manager],
            roles);
        Assert.DoesNotContain(TenantRoles.Driver, roles);
        Assert.DoesNotContain(TenantRoles.Customer, roles);
    }

    /// <summary>
    /// Defaults apply to every future tenant, and Permission.Tenant.Manage is granted to every
    /// tenant's own Owner, so these two must be role-gated rather than policy-gated.
    /// </summary>
    [Theory]
    [InlineData(nameof(FeaturesController.GetDefaultFeatures))]
    [InlineData(nameof(FeaturesController.UpdateDefaultFeatures))]
    public void Default_feature_endpoints_are_platform_admin_only(string action)
    {
        var roles = RolesOn(typeof(FeaturesController), action);

        Assert.Equal([AppRoles.SuperAdmin, AppRoles.Admin], roles);
    }

    /// <summary>
    /// Drivers hold Load.View for their own assignments, so the unassigned board is gated on
    /// Dispatch.View - the same permission its AI tool requires.
    /// </summary>
    [Fact]
    public void Unassigned_loads_require_dispatch_view()
    {
        var policy = typeof(LoadController)
            .GetMethod(nameof(LoadController.GetUnassignedLoads), BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>()?.Policy;

        Assert.Equal(Permission.Dispatch.View, policy);
    }
}
