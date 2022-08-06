using System.Security.Claims;

namespace Logistics.OfficeApp;

public static class ClaimsExtensions
{
    public static EmployeeDto ToUser(this IEnumerable<Claim> claims)
    {
        var employee = new EmployeeDto();

        foreach (var claim in claims)
        {
            switch (claim.Type)
            {
                case ClaimTypes.GivenName:
                    employee.FirstName = claim.Value;
                    break;
                case ClaimTypes.Surname:
                    employee.LastName = claim.Value;
                    break;
                case ClaimTypes.Name:
                    employee.UserName = claim.Value;
                    break;
                case ClaimTypes.NameIdentifier:
                    employee.Id = claim.Value;
                    break;
                case "sub":
                    employee.Id = claim.Value;
                    break;
            }
        }

        return employee;
    }

    public static string? GetId(this ClaimsPrincipal user)
    {
        return user.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
