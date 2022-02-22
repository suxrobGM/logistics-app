using System.Security.Claims;

namespace Logistics.AdminApp;

public static class ClaimsExtensions
{
    public static UserDto ToUser(this IEnumerable<Claim> claims)
    {
        var user = new UserDto();

        foreach (var claim in claims)
        {
            //if (claim.Type == ClaimTypes.Obje)
            //{
                
            //}
        }

        return user;
    }
}
