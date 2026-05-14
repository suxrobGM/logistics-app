using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class ValidateInvitationTokenQuery : IQuery<Result<InvitationValidationResult>>
{
    public required string Token { get; set; }
}
