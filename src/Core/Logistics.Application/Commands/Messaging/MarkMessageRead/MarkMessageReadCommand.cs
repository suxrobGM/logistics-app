using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class MarkMessageReadCommand : IAppRequest
{
    public Guid MessageId { get; set; }
    public Guid ReadById { get; set; }
}
