namespace Logistics.Application.Contracts.Models;

public class UserDto
{
    public string? Id { get; set; }

    private string? _userName;
    public string? UserName
    { 
        get => string.IsNullOrEmpty(_userName) ? 
            $"{FirstName} {LastName}" : _userName;

        set => _userName = string.IsNullOrEmpty(_userName) ?
            $"{FirstName} {LastName}" : value;
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
}
