using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class AcceptInvitationCommand : IAppRequest<Result<AcceptInvitationResult>>
{
    public required string Token { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Password { get; set; }
}
