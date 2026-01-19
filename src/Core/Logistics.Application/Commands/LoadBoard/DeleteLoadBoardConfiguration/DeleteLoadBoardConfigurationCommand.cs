using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteLoadBoardConfigurationCommand : IAppRequest
{
    public Guid Id { get; set; }
}
