using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class MapEldDriverHandler(
    ITenantUnitOfWork tenantUow,
    ILogger<MapEldDriverHandler> logger)
    : IAppRequestHandler<MapEldDriverCommand, Result>
{
    public async Task<Result> Handle(MapEldDriverCommand req, CancellationToken ct)
    {
        // Verify employee exists
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);
        if (employee is null)
        {
            return Result.Fail($"Employee with ID {req.EmployeeId} not found");
        }

        // Check if mapping already exists for this employee and provider
        var existingMapping = await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.EmployeeId == req.EmployeeId && m.ProviderType == req.ProviderType);

        if (existingMapping is not null)
        {
            return Result.Fail(
                $"Employee {employee.GetFullName()} is already mapped to an ELD driver for this provider");
        }

        // Check if external driver is already mapped to another employee
        var externalMapping = await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.ExternalDriverId == req.ExternalDriverId && m.ProviderType == req.ProviderType);

        if (externalMapping is not null)
        {
            return Result.Fail($"ELD driver {req.ExternalDriverId} is already mapped to another employee");
        }

        // Create the mapping
        var mapping = new EldDriverMapping
        {
            EmployeeId = req.EmployeeId,
            ProviderType = req.ProviderType,
            ExternalDriverId = req.ExternalDriverId,
            ExternalDriverName = req.ExternalDriverName,
            IsSyncEnabled = true
        };

        await tenantUow.Repository<EldDriverMapping>().AddAsync(mapping);
        await tenantUow.SaveChangesAsync();

        logger.LogInformation("Mapped employee {EmployeeId} to ELD driver {ExternalDriverId} for {ProviderType}",
            req.EmployeeId, req.ExternalDriverId, req.ProviderType);

        return Result.Ok();
    }
}
