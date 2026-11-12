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
            .Where(m => !m.IsSpecialName && m.GetCustomAttributes<HttpMethodAttribute>().Any());
    }

    private static string[] RolesOn(Type controller, string action)
    {
        var attribute = controller.GetMethod(action, BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>();

        return attribute?.Roles?.Split(',', StringSplitOptions.TrimEntries) ?? [];
    }

    /// <summary>Still authenticated by the global AuthorizeFilter. The list must shrink, never grow.</summary>
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

    /// <summary>Reachable without signing in, so each must protect itself: a webhook signature, a token in the route, or public content.</summary>
    private static readonly string[] AnonymousActions =
    [
        "AccountingController.Callback",
        "AccountingController.Disconnected",
        "BlogPostController.GetPublishedBlogPostBySlug",
        "BlogPostController.GetPublishedBlogPosts",
        "ContactController.CreateContactSubmission",
        "DefaultController.Get",
        "DemoRequestController.CreateDemoRequest",
        "InvitationController.Accept",
        "InvitationController.ValidateToken",
        "ProductLicenseController.GetProductLicenseDiscovery",
        "ProductLicenseController.RecordProductLicenseHeartbeat",
        "PublicPaymentController.CreateCheckoutSession",
        "PublicPaymentController.GetInvoice",
        "TrackingController.DownloadDocument",
        "TrackingController.GetDocuments",
        "TrackingController.GetPublicTracking",
        "WebhookController.GeotabWebhook",
        "WebhookController.MotiveWebhook",
        "WebhookController.Resend",
        "WebhookController.SamsaraWebhook",
        "WebhookController.Stripe"
    ];

    [Fact]
    public void Anonymous_actions_are_an_explicit_allowlist()
    {
        var anonymous = Controllers
            .SelectMany(c => ActionsOf(c).Select(a => (Controller: c, Action: a)))
            .Where(x =>
                x.Action.GetCustomAttribute<AllowAnonymousAttribute>() is not null ||
                x.Controller.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
            .Select(x => $"{x.Controller.Name}.{x.Action.Name}")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(AnonymousActions.OrderBy(n => n, StringComparer.Ordinal).ToArray(), anonymous);
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
    public void Feature_admin_endpoints_are_platform_admin_only()
    {
        string[] actions =
        [
            nameof(FeaturesController.GetTenantFeatures),
            nameof(FeaturesController.UpdateTenantFeatures)
        ];

        foreach (var action in actions)
        {
            Assert.Equal([AppRoles.SuperAdmin, AppRoles.Admin], RolesOn(typeof(FeaturesController), action));
        }
    }

    [Fact]
    public void Tenant_presets_and_tenant_features_are_platform_admin_only()
    {
        (Type Controller, string Action)[] actions =
        [
            (typeof(TenantController), nameof(TenantController.CreateTenant)),
            (typeof(TenantController), nameof(TenantController.UpdateTenantPresets)),
            (typeof(FeaturesController), nameof(FeaturesController.GetTenantFeatures)),
            (typeof(FeaturesController), nameof(FeaturesController.UpdateTenantFeatures))
        ];

        foreach (var (controller, action) in actions)
        {
            Assert.Equal([AppRoles.SuperAdmin, AppRoles.Admin], RolesOn(controller, action));
        }
    }

    /// <summary>Every role holds User.View; your own profile goes through /users/me.</summary>
    [Fact]
    public void Updating_another_user_requires_user_manage()
    {
        var policy = typeof(UserController)
            .GetMethod(nameof(UserController.Update), BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>()?.Policy;

        Assert.Equal(Permission.User.Manage, policy);
    }

    /// <summary>Every role holds User.View; your own profile comes from /users/me.</summary>
    [Fact]
    public void Reading_another_user_requires_user_manage()
    {
        var policy = typeof(UserController)
            .GetMethod(nameof(UserController.GetById), BindingFlags.Public | BindingFlags.Instance)
            ?.GetCustomAttribute<AuthorizeAttribute>()?.Policy;

        Assert.Equal(Permission.User.Manage, policy);
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
