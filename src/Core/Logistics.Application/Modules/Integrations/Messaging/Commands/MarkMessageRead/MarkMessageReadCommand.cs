using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

public class MarkMessageReadCommand : ICommand
{
    public Guid MessageId { get; set; }
    public Guid ReadById { get; set; }
}
