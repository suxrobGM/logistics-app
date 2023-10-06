using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetLoadsQuery : SearchableQuery<LoadDto>
{
    public bool LoadAllPages { get; set; }
    public bool OnlyActiveLoads { get; set; }
    public string? UserId { get; set; }
    public string? TruckId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
