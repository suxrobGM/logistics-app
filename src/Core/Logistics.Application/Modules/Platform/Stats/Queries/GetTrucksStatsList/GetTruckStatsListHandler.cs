using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

internal sealed class GetTruckStatsListHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrucksStatsListQuery, PagedResult<TruckStatsDto>>
{
    public async Task<PagedResult<TruckStatsDto>> Handle(
        GetTrucksStatsListQuery req, CancellationToken ct)
    {
        var totalsByTruck = tenantUow.Repository<Load>().Query()
            .Where(l => l.AssignedTruckId != null && l.DeliveredAt >= req.StartDate && l.DeliveredAt <= req.EndDate)
            .GroupBy(l => l.AssignedTruckId!.Value)
            .Select(g => new
            {
                TruckId = g.Key,
                Gross = g.Sum(l => l.DeliveryCost.Amount),
                Distance = g.Sum(l => l.Distance)
            });

        // The sort string resolves against TruckStatsRow, so only its columns are sortable.
        var rows = tenantUow.Repository<Truck>().Query()
            .Join(totalsByTruck, t => t.Id, s => s.TruckId, (t, s) => new TruckStatsRow
            {
                TruckId = t.Id,
                TruckNumber = t.Number,
                Gross = s.Gross,
                Distance = s.Distance,
                DriverShare = s.Gross * (
                    (t.MainDriver != null && t.MainDriver.SalaryType == SalaryType.ShareOfGross
                        ? t.MainDriver.Salary.Amount
                        : 0m) +
                    (t.SecondaryDriver != null && t.SecondaryDriver.SalaryType == SalaryType.ShareOfGross
                        ? t.SecondaryDriver.Salary.Amount
                        : 0m)),
                MainDriverId = t.MainDriverId,
                SecondaryDriverId = t.SecondaryDriverId
            });

        var totalItems = await rows.CountAsync(ct);
        var page = await rows
            .OrderBy(req.OrderBy, r => r.TruckId, defaultOrderBy: nameof(TruckStatsRow.TruckNumber))
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var driverIds = page
            .SelectMany(r => new[] { r.MainDriverId, r.SecondaryDriverId })
            .OfType<Guid>()
            .Distinct()
            .ToArray();
        var drivers = driverIds.Length == 0
            ? []
            : (await tenantUow.Repository<Employee>().Query()
                .Where(e => driverIds.Contains(e.Id))
                .Select(e => new { Employee = e, e.Role })
                .ToListAsync(ct))
                .ToDictionary(x => x.Employee.Id, x => x.Employee.ToDto(x.Role));

        var items = page.Select(r => new TruckStatsDto
        {
            TruckId = r.TruckId,
            TruckNumber = r.TruckNumber,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Gross = r.Gross,
            Distance = r.Distance,
            DriverShare = r.DriverShare,
            Drivers = [.. new[] { r.MainDriverId, r.SecondaryDriverId }
                .Select(id => id is { } key ? drivers.GetValueOrDefault(key) : null)
                .OfType<EmployeeDto>()],
            TotalItems = totalItems
        }).ToList();

        return PagedResult<TruckStatsDto>.Ok(items, totalItems, req.PageSize);
    }

    private sealed record TruckStatsRow
    {
        public Guid TruckId { get; init; }
        public required string TruckNumber { get; init; }
        public decimal Gross { get; init; }
        public double Distance { get; init; }
        public decimal DriverShare { get; init; }
        public Guid? MainDriverId { get; init; }
        public Guid? SecondaryDriverId { get; init; }
    }
}
