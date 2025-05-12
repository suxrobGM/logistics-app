using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class ConfirmLoadStatus
{
    public Guid? DriverId { get; set; }
    public Guid? LoadId { get; set; }
    public LoadStatus LoadStatus { get; set; }
}
