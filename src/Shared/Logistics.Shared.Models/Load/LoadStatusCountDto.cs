using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class LoadStatusCountDto
{
    public LoadStatus Status { get; set; }
    public int Count { get; set; }
}
