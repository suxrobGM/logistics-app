using System.Security.Claims;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Logistics.Infrastructure.Communications.Tests.SignalR;

internal static class HubTestContext
{
    public const string ConnectionId = "conn-1";

    /// <summary>A caller context carrying the tenant and user claims every hub method reads.</summary>
    public static HubCallerContext Caller(Guid tenantId, Guid userId)
    {
        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);
        context.Items.Returns(new Dictionary<object, object?>());
        context.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(CustomClaimTypes.Tenant, tenantId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ])));
        return context;
    }
}
