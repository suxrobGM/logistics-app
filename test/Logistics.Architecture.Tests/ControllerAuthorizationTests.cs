using System.Reflection;
using Logistics.API.Controllers;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Logistics.Architecture.Tests;

public class ControllerAuthorizationTests
{
    private static string[] RolesOn(Type controller, string action)
    {
        var attribute = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>();

        return attribute?.Roles?.Split(',', StringSplitOptions.TrimEntries) ?? [];
    }

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

    [Theory]
    [InlineData(nameof(FeaturesController.GetDefaultFeatures))]
    [InlineData(nameof(FeaturesController.UpdateDefaultFeatures))]
    public void Default_feature_endpoints_are_platform_admin_only(string action)
    {
        var roles = RolesOn(typeof(FeaturesController), action);

        Assert.Equal([AppRoles.SuperAdmin, AppRoles.Admin], roles);
    }

    [Fact]
    public void Unassigned_loads_require_dispatch_view()
    {
        var policy = typeof(LoadController)
            .GetMethod(nameof(LoadController.GetUnassignedLoads), BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>()?.Policy;

        Assert.Equal(Permission.Dispatch.View, policy);
    }
}
