namespace Logistics.Application.Main.Models;

public class UserDto
{
    public UserDto()
    {
        UserName = string.Empty;
    }

    public string? Id { get; set; }
    public string UserName { get; set; }
    public string? FirstName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
        {
            return UserName;
        }
        return string.Join(" ", FirstName, LastName);
    }
}
