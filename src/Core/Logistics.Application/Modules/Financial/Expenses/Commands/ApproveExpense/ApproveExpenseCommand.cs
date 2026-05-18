using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class ApproveExpenseCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public required string ApproverId { get; set; }
}
