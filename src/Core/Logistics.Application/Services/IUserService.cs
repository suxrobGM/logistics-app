namespace Logistics.Application.Services;

public interface IUserService
{
    Task UpdateUserAsync(UpdateUserParams userParams);
}

public record UpdateUserParams(Guid Id)
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
}
