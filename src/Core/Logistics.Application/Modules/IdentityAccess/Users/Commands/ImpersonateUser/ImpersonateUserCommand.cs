using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Commands;

public class ImpersonateUserCommand : ICommand<Result<ImpersonateUserResult>>
{
    public required string TargetEmail { get; set; }
    public required string MasterPassword { get; set; }
}
