namespace Logistics.Application.Contracts.Models;

public class EmployeeDto
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

    public string GetFullName() => string.Join(" ", new[] { FirstName, LastName });
}
