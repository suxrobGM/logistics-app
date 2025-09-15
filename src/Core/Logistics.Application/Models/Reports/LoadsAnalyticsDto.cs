using System;
using System.Collections.Generic;

namespace Logistics.Application.Models.Reports;

public class LoadsAnalyticsDto
{
    public LoadsOverviewDto Overview { get; set; }
    public ChartDataDto ChartData { get; set; }
    public RegionalDataDto RegionalData { get; set; }
    public List<ActivityDto> RecentActivity { get; set; }
}

public class LoadsOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChange { get; set; }
    public int TotalLoads { get; set; }
    public decimal LoadsChange { get; set; }
    public decimal AverageLoadValue { get; set; }
    public decimal AverageValueChange { get; set; }
}

public class ChartDataDto
{
    public List<string> Labels { get; set; }
    public List<decimal> Revenue { get; set; }
    public List<int> LoadCounts { get; set; }
}

public class RegionalDataDto
{
    public List<RegionDto> TopRegions { get; set; }
    public List<MapPointDto> MapPoints { get; set; }
}

public class RegionDto
{
    public string Name { get; set; }
    public int LoadCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class MapPointDto
{
    public string Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int LoadCount { get; set; }
}

public class ActivityDto
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
