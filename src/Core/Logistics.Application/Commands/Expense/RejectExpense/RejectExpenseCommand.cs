using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RejectExpenseCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public required string ApproverId { get; set; }
    public required string Reason { get; set; }
}
