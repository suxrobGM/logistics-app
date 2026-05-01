using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class DeleteContainerCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
}
