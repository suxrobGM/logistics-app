using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

/// <summary>Sets the current tenant's default detention terms.</summary>
public class UpdateDetentionTermsCommand : ICommand<Result>
{
    public int? FreeMinutes { get; set; }
    public decimal? RatePerHour { get; set; }
}
