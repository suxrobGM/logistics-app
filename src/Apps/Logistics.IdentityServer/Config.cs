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
            new()
            {
                Name = "roles",
                DisplayName = "Identity roles",
                UserClaims = {
                    ClaimTypes.Role, 
                    ClaimTypes.Permission
                }
            }
        };
    }
    
    public static IEnumerable<ApiScope> ApiScopes(IConfiguration configuration)
    {
        return new ApiScope[]
        {
            new()
            {
                Name = "logistics.api.admin",
                DisplayName = "Logistics Admin API",
                UserClaims = {
                    ClaimTypes.Role, 
                    ClaimTypes.Permission
                }
            },
            new()
            {
                Name = "logistics.api.tenant",
                DisplayName = "Logistics Tenant API",
                UserClaims = {
                    ClaimTypes.Role, 
                    ClaimTypes.Permission, 
                    ClaimTypes.Tenant
                }
            }
        };
    }

    public static IEnumerable<ApiResource> ApiResources(IConfiguration configuration)
    {
        return new ApiResource[]
        {
            new()
            {
                Name = "logistics.api",
                DisplayName = "Logistics API",
                Scopes = {
                    "logistics.api.admin",
                    "logistics.api.tenant"
                },
                UserClaims = {
                    ClaimTypes.Role, 
                    ClaimTypes.Permission, 
                    ClaimTypes.Tenant
                }
            }
        };
    }

    public static IEnumerable<Client> Clients(IConfiguration configuration)
    {
        //var a = "Super secret key 1".Sha256();
        //var b = "Super secret key 2".Sha256();
        return configuration.GetSection("IdentityServer:Clients").Get<Client[]>();
    }
}