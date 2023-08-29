namespace Logistics.Models;

public class EmployeeDto
{
    public EmployeeDto()
    {
        UserName = string.Empty;
        Id = string.Empty;
    }
    
    public EmployeeDto(
        string id, 
        string userName, 
        string? firstName, 
        string? lastName, 
        string email, 
        string? phoneNumber, 
        DateTime joinedDate)
    {
        Id = id;
        UserName = userName;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        JoinedDate = joinedDate;
    }
    
    public string Id { get; set; }
    public string UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public List<TenantRoleDto> Roles { get; set; } = new();
}
