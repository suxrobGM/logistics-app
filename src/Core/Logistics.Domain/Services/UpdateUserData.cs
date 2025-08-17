namespace Logistics.Domain.Services;

public record UpdateUserData(Guid Id)
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
}
