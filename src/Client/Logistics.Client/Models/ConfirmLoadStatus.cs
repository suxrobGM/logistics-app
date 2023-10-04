using Logistics.Models;

namespace Logistics.Client.Models;

public class ConfirmLoadStatus
{
    public string? LoadId { get; set; }
    public LoadStatusDto LoadStatus { get; set; }
}
