using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DownloadExpenseReceiptQuery : IAppRequest<Result<DocumentDownloadDto>>
{
    public Guid ExpenseId { get; set; }
    public Guid RequestedById { get; set; }
}
