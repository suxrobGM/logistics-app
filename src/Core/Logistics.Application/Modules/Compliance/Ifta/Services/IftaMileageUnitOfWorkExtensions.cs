using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

public static class IftaMileageUnitOfWorkExtensions
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>Metres each truck drove in [from, to) per the IFTA mileage log, keyed by truck.</summary>
        public async Task<Dictionary<Guid, double>> SumLoggedMetersByTruckAsync(
            DateTime from, DateTime to, CancellationToken ct)
        {
            var fromDay = DateOnly.FromDateTime(from);
            var toDay = DateOnly.FromDateTime(to);
            var logged = await tenantUow.Repository<TruckJurisdictionMileage>().Query()
                .Where(m => m.Date >= fromDay && m.Date < toDay)
                .GroupBy(m => m.TruckId)
                .Select(g => new { TruckId = g.Key, Meters = g.Sum(m => m.Meters) })
                .ToListAsync(ct);
            return logged.ToDictionary(m => m.TruckId, m => (double)m.Meters);
        }
    }
}
