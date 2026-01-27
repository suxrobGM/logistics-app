namespace Logistics.Shared.Models;

public class CompanyStatsDto
{
    public string? OwnerName { get; set; }
    public int EmployeesCount { get; set; }
    public int ManagersCount { get; set; }
    public int DispatchersCount { get; set; }
    public int DriversCount { get; set; }
    public int TrucksCount { get; set; }

    public decimal ThisWeekGross { get; set; }
    public double ThisWeekDistance { get; set; }

    public decimal LastWeekGross { get; set; }
    public double LastWeekDistance { get; set; }

    public decimal ThisMonthGross { get; set; }
    public double ThisMonthDistance { get; set; }

    public decimal LastMonthGross { get; set; }
    public double LastMonthDistance { get; set; }

    public decimal LastThreeMonthsGross { get; set; }
    public double LastThreeMonthsDistance { get; set; }

    public decimal LastYearGross { get; set; }
    public double LastYearDistance { get; set; }

    public decimal TotalGross { get; set; }
    public double TotalDistance { get; set; }

    // Dashboard KPIs
    public int ActiveLoadsCount { get; set; }
    public int UnassignedLoadsCount { get; set; }
    public int IdleTrucksCount { get; set; }

    // Financial Health
    public decimal OutstandingInvoiceTotal { get; set; }
    public decimal PaymentsReceivedThisWeek { get; set; }
    public int OverdueInvoiceCount { get; set; }

    // Top Performers
    public List<TopTruckDto>? TopTrucks { get; set; }
}

public class TopTruckDto
{
    public Guid TruckId { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriverName { get; set; }
    public decimal Revenue { get; set; }
}
