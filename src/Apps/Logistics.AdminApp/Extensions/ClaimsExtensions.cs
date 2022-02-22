using System.Security.Claims;

namespace Logistics.AdminApp;

public static class ClaimsExtensions
{
    public static UserDto ToUser(this IEnumerable<Claim> claims)
    {
        var user = new UserDto();

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
            else if (claim.Type == "emails")
            {
                user.Email = claim.Value;
            }
            else if (claim.Type == "name")
            {
                user.UserName = claim.Value;
            }
        }

        return user;
    }
}
