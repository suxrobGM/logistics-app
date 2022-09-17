using System.Text.Json.Serialization;

namespace Logistics.Application.Contracts.Models;

public class TruckGrossesDto
{
    public string? TruckId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public GrossesForIntervalDto? Grosses { get; set; }

    public decimal TotalGrossAllTime { get; set; }
    public double TotalDistanceAllTime { get; set; }
}