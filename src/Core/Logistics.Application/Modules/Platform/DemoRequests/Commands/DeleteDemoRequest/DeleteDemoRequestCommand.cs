using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Commands;

public sealed class DeleteDemoRequestCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
