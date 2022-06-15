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
    
    public static IEnumerable<ApiScope> ApiScopes(IConfiguration configuration)
    {
        return configuration.GetSection("IdentityServer:ApiScopes").Get<ApiScope[]>();
    }

    public static IEnumerable<ApiResource> ApiResources(IConfiguration configuration)
    {
        return configuration.GetSection("IdentityServer:ApiResources").Get<ApiResource[]>();
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        return configuration.GetSection("IdentityServer:Clients").Get<Client[]>();
    }
}