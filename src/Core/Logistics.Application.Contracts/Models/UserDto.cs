namespace Logistics.Application.Contracts.Models;

public class UserDto
{
    public string? Id { get; set; }

    private string? _userName;
    public string? UserName
    { 
        get => string.IsNullOrEmpty(_userName) ?
            GetFullName() : _userName;

        set => _userName = string.IsNullOrEmpty(value) ?
            GetFullName() : value;
    }

    [Required]
    public string? ExternalId { get; set; }

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";
}
