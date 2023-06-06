namespace Logistics.Client.Models;

public record User
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}
