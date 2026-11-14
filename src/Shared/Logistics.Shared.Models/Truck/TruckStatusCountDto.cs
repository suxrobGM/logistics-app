using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class TruckStatusCountDto
{
    public TruckStatus Status { get; set; }
    public int Count { get; set; }
}
