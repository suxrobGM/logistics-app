using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteEldDriverMappingCommand : IAppRequest
{
    public Guid MappingId { get; set; }
}
