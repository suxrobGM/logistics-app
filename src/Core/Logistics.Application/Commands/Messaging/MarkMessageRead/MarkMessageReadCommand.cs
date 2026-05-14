using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class MarkMessageReadCommand : ICommand
{
    public Guid MessageId { get; set; }
    public Guid ReadById { get; set; }
}
