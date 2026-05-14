using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteSubscriptionCommand : ICommand
{
    public Guid Id { get; set; }
}
