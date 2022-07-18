namespace Logistics.Application.Contracts.Models;

public class EmployeeDto
{
    public EmployeeDto()
    {
        UserName = string.Empty;
        ExternalId = string.Empty;
    }

    public string? Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string ExternalId { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
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
