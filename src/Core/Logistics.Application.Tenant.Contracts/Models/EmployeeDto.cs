namespace Logistics.Application.Contracts.Models;

public class EmployeeDto
{
    public EmployeeDto()
    {
        UserName = string.Empty;
        Id = string.Empty;
    }
    
    [Required]
    public string Id { get; set; }
    
    public string UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime JoinedDate { get; set; }

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName;
        }
        return string.Join(" ", FirstName, LastName);
    }
}
