using Duende.IdentityServer.Models;
using IdentityModel;

namespace Logistics.IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources()
    {
        return new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new()
            {
                Name = "roles",
                DisplayName = "Identity roles",
                UserClaims = { JwtClaimTypes.Role }
            }
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