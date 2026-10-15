using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

/// <summary>Sets a customer's own detention terms; null fields fall back to the tenant's.</summary>
public class UpdateCustomerDetentionTermsCommand : ICommand<Result>
{
    public Guid CustomerId { get; set; }
    public int? FreeMinutes { get; set; }
    public decimal? RatePerHour { get; set; }
}
