namespace Logistics.Application.Contracts.Models;

public class EmployeeRoleDto
{
    public EmployeeRoleDto(string userId, string role)
    {
        UserId = userId;
        Role = role;
    }

    public string UserId { get; set; }
    public string Role { get; set; }
}
