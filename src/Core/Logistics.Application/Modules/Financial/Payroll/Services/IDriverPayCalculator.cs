using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

/// <summary>Driver pay rules shared by payroll and profitability, so the two never disagree.</summary>
public interface IDriverPayCalculator : IApplicationService
{
    /// <summary>Pay earned on the given loads by a per-load salary type; 0 for period-paid types.</summary>
    decimal PayForLoads(Employee employee, IReadOnlyCollection<LoadPayBasis> loads, DistanceUnit unit);

    /// <summary>Pay for a period by a Weekly, Monthly or Hourly type, or the base pay of an untyped salary; 0 for per-load types.</summary>
    decimal PayForPeriod(Employee employee, decimal weeks, decimal months, decimal hours);
}

/// <param name="Distance">Metres.</param>
public sealed record LoadPayBasis(
    decimal Linehaul, decimal FuelSurcharge, decimal Accessorials, double Distance, int DepartedStops = 0);
