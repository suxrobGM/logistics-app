using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class DeleteExpenseCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
