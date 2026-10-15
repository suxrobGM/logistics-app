using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// Record reserve the factor paid out on a funded invoice after the customer paid it.
/// </summary>
public class ReleaseFactoringReserveCommand : ICommand<Result>
{
    public Guid BatchId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ReleasedAt { get; set; }
}
