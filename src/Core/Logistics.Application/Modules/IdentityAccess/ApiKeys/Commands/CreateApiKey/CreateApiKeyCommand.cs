using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.ApiKeys.Commands;

public sealed class CreateApiKeyCommand : ICommand<Result<ApiKeyCreatedDto>>
{
    public string Name { get; set; } = string.Empty;
}
