namespace Logistics.Application.Modules.IdentityAccess.Users.Services;

public interface IUserService : IApplicationService
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
