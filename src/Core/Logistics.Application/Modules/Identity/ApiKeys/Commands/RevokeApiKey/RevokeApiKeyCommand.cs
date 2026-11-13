using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.ApiKeys.Commands;

public sealed class RevokeApiKeyCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
