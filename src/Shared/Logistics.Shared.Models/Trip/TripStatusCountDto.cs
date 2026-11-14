using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class TripStatusCountDto
{
    public TripStatus Status { get; set; }
    public int Count { get; set; }
}
