using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.DemoRequests.Commands;

public sealed class DeleteDemoRequestCommand : ICommand<Result>, IHaveId
{
    public Guid Id { get; set; }
}
