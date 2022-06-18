using System.Security.Claims;

namespace Logistics.OfficeApp;

public static class ClaimsExtensions
{
    public static EmployeeDto ToUser(this IEnumerable<Claim> claims)
    {
        var user = new EmployeeDto();

        foreach (var claim in claims)
        {
            switch (claim.Type)
            {
                case ClaimTypes.GivenName:
                    user.FirstName = claim.Value;
                    break;
                case ClaimTypes.Surname:
                    user.LastName = claim.Value;
                    break;
                case ClaimTypes.Name:
                    user.UserName = claim.Value;
                    break;
                case "sub":
                    user.ExternalId = claim.Value;
                    break;
            }
        }

        return user;
    }

    public static string? GetId(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
