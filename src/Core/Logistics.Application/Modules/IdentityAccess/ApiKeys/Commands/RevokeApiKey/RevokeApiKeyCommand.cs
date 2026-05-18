using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.ApiKeys.Commands;

public sealed class RevokeApiKeyCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
