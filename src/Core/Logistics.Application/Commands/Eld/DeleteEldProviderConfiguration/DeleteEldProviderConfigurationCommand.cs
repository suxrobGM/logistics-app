using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteEldProviderConfigurationCommand : IAppRequest
{
    public Guid ProviderId { get; set; }
}
