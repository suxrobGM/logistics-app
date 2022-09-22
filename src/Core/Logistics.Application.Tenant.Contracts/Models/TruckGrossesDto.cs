﻿using System.Text.Json.Serialization;

namespace Logistics.Application.Contracts.Models;

public class TruckGrossesDto
{
    public string? TruckId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DailyGrossesDto? Grosses { get; set; }

    public double TotalGrossAllTime { get; set; }
    public double TotalDistanceAllTime { get; set; }
}