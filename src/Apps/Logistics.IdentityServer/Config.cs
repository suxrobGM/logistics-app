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
        return new ApiScope[]
        {
            new("logistics.api.admin", "Logistics Admin API")
            {
                UserClaims = {
                    "role",
                }
            },
            new("logistics.api.tenant", "Logistics Tenant API")
            {
                UserClaims = {
                    "role",
                    "tenant"
                }
            }
        };
    }

    public static IEnumerable<ApiResource> ApiResources(IConfiguration configuration)
    {
        return new ApiResource[]
        {
            new("logistics.api", "Logistics API")
            {
                Scopes = {
                    "logistics.api.admin",
                    "logistics.api.tenant"
                },
                UserClaims = {
                    "role",
                    "tenant"
                }
            }
        };
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        return configuration.GetSection("IdentityServer:Clients").Get<Client[]>();
    }
}