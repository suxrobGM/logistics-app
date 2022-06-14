using System.Security.Claims;

namespace Logistics.OfficeApp;

public static class ClaimsExtensions
{
    public static EmployeeDto ToUser(this IEnumerable<Claim> claims)
    {
        var user = new EmployeeDto();

        foreach (var claim in claims)
        {
            if (claim.Type == ClaimTypes.NameIdentifier)
            {
                user.ExternalId = claim.Value;
            }
            else if (claim.Type == ClaimTypes.GivenName)
            {
                user.FirstName = claim.Value;
            }
            else if (claim.Type == ClaimTypes.Surname)
            {
                user.LastName = claim.Value;
            }
            else if (claim.Type == "name")
            {
                user.UserName = claim.Value;
            }
        }

        return user;
    }

    public static string? GetId(this ClaimsPrincipal user)
    {
        return user?.Claims?.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
