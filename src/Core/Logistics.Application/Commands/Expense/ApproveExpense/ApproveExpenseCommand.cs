using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class ApproveExpenseCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public required string ApproverId { get; set; }
}
