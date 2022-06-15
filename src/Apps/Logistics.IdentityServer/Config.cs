using Duende.IdentityServer.Models;

namespace Logistics.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources()
    {
        return new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
    }

    public static IEnumerable<ApiScope> ApiScopes()
    {
        return new[]
        {
            new ApiScope("admin.read"),
            new ApiScope("admin.write"),
            new ApiScope("tenant.read"),
            new ApiScope("tenant.write")
        };
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        return configuration.GetSection("IdentityServer:Clients").Get<Client[]>();;
    }
}