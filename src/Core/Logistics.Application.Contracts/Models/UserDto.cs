using System.ComponentModel.DataAnnotations;

namespace Logistics.Application.Contracts.Models;

public class UserDto
{
    [Required]
    public string? ExternalId { get; init; }

    [Required]
    public string? FirstName { get; init; }

    [Required]
    public string? LastName { get; init; }

    [Required, EmailAddress]
    public string? Email { get; init; }

    [Required, Phone]
    public string? PhoneNumber { get; init; }
}
