using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class ImpersonateUserCommand : IAppRequest<Result<ImpersonateUserResult>>
{
    public required string TargetEmail { get; set; }
    public required string MasterPassword { get; set; }
}
