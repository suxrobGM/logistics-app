using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

public class DownloadExpenseReceiptQuery : IQuery<Result<DocumentDownloadDto>>
{
    public Guid ExpenseId { get; set; }
    public Guid RequestedById { get; set; }
}
