using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Expenses.Commands;

public class UploadExpenseReceiptCommand : ICommand<Result<string>>
{
    public required Stream FileContent { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
}
