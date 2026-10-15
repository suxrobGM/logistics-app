using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// Put invoices on a schedule of accounts and email it, with each invoice PDF and its load paperwork,
/// to the factoring company their customers are assigned to.
/// </summary>
public class SubmitInvoicesToFactorCommand : ICommand<Result<FactoringBatchDto>>
{
    public List<Guid> InvoiceIds { get; set; } = [];

    public string? Notes { get; set; }
}
