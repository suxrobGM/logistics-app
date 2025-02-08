using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantHandler : RequestHandler<UpdateTenantCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public UpdateTenantHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(UpdateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Domain.Entities.Tenant>().GetByIdAsync(req.Id);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(req.Name) && tenant.Name != req.Name)
        {
            tenant.Name = req.Name.Trim().ToLower();
        }
        if (!string.IsNullOrEmpty(req.CompanyName) && tenant.CompanyName != req.CompanyName)
        {
            tenant.CompanyName = req.CompanyName;
        }
        if (req.CompanyAddress != null && tenant.CompanyAddress != req.CompanyAddress)
        {
            tenant.CompanyAddress = req.CompanyAddress;
        }
        if (!string.IsNullOrEmpty(req.ConnectionString) && tenant.ConnectionString != req.ConnectionString)
        {
            tenant.ConnectionString = req.ConnectionString;
        }
        
        _masterUow.Repository<Domain.Entities.Tenant>().Update(tenant);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
