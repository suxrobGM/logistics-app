using System.Reflection;
using Logistics.API.Controllers;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Logistics.Architecture.Tests;

public class ControllerAuthorizationTests
{
    private static readonly Type[] Controllers = typeof(LoadController).Assembly
        .GetTypes()
        .Where(t => t is { IsAbstract: false, IsPublic: true } && typeof(ControllerBase).IsAssignableFrom(t))
        .ToArray();

    private static IEnumerable<MethodInfo> ActionsOf(Type controller)
    {
        return controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialAccessor() && m.GetCustomAttributes<HttpMethodAttribute>().Any());
    }

    private static string[] RolesOn(Type controller, string action)
    {
        var attribute = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>();

        return attribute?.Roles?.Split(',', StringSplitOptions.TrimEntries) ?? [];
    }

    /// <summary>
    /// Actions that carry no permission of their own. The global AuthorizeFilter still requires an
    /// authenticated caller, so these are not anonymous. The list must shrink, never grow.
    /// </summary>
    private static readonly string[] AuthenticatedOnlyActions = ["RoleController.GetPermissions"];

    [Fact]
    public void Every_action_declares_its_authorization()
    {
        var unguarded = Controllers
            .SelectMany(c => ActionsOf(c).Select(a => (Controller: c, Action: a)))
            .Where(x =>
                x.Action.GetCustomAttribute<AllowAnonymousAttribute>() is null &&
                x.Controller.GetCustomAttribute<AllowAnonymousAttribute>() is null &&
                x.Action.GetCustomAttribute<AuthorizeAttribute>() is null &&
                x.Controller.GetCustomAttribute<AuthorizeAttribute>() is null)
            .Select(x => $"{x.Controller.Name}.{x.Action.Name}")
            .Except(AuthenticatedOnlyActions)
            .OrderBy(name => name)
            .ToArray();

        Assert.Empty(unguarded);
    }

    [Fact]
    public void Billing_mutations_are_limited_to_billing_roles()
    {
        string[] mutations =
        [
            nameof(SubscriptionController.CancelSubscription),
            nameof(SubscriptionController.ChangeSubscriptionPlan),
            nameof(SubscriptionController.RenewSubscription)
        ];

        foreach (var action in mutations)
        {
            var roles = RolesOn(typeof(SubscriptionController), action);

            Assert.Equal(
                [AppRoles.SuperAdmin, AppRoles.Admin, TenantRoles.Owner, TenantRoles.Manager],
                roles);
            Assert.DoesNotContain(TenantRoles.Driver, roles);
            Assert.DoesNotContain(TenantRoles.Customer, roles);
        }
    }

    [Fact]
    public void Default_feature_endpoints_are_platform_admin_only()
    {
        string[] actions =
        [
            nameof(FeaturesController.GetDefaultFeatures),
            nameof(FeaturesController.UpdateDefaultFeatures)
        ];

        foreach (var action in actions)
        {
            Assert.Equal([AppRoles.SuperAdmin, AppRoles.Admin], RolesOn(typeof(FeaturesController), action));
        }
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

file static class MethodInfoExtensions
{
    public static bool IsSpecialAccessor(this MethodInfo method)
    {
        return method.IsSpecialName || method.DeclaringType == typeof(object);
    }
}
