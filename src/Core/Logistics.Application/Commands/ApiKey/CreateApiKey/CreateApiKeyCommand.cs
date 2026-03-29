using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class CreateApiKeyCommand : IAppRequest<Result<ApiKeyCreatedDto>>
{
    public string Name { get; set; } = string.Empty;
}
