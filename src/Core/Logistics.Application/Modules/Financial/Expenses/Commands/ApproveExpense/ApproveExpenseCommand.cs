using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class ApproveExpenseCommand : ICommand<Result>
{
    public Guid Id { get; set; }
    public required Guid ApproverId { get; set; }
}
