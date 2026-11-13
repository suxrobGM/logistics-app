using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Users.Commands;

public class ImpersonateUserCommand : ICommand<Result<ImpersonateUserResult>>
{
    public required string TargetEmail { get; set; }
    public required string MasterPassword { get; set; }
}
