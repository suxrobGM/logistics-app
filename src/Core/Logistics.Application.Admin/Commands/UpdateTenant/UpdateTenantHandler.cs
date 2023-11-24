using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateTenantHandler : RequestHandler<UpdateTenantCommand, ResponseResult>
{
    private readonly IMasterUnityOfWork _masterUow;

    public UpdateTenantHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<ResponseResult> HandleValidated(UpdateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.Id);

        if (tenant is null)
        {
            return ResponseResult.CreateError($"Could not find a tenant with ID '{req.Id}'");
        }

        if (!string.IsNullOrEmpty(req.Name) && tenant.Name != req.Name)
        {
            tenant.Name = req.Name.Trim().ToLower();
        }
        if (!string.IsNullOrEmpty(req.CompanyName) && tenant.CompanyName != req.CompanyName)
        {
            tenant.CompanyName = req.CompanyName;
        }
        if (!string.IsNullOrEmpty(req.CompanyAddress) && tenant.CompanyAddress != req.CompanyAddress)
        {
            tenant.CompanyAddress = req.CompanyAddress;
        }
        if (!string.IsNullOrEmpty(req.ConnectionString) && tenant.ConnectionString != req.ConnectionString)
        {
            tenant.ConnectionString = req.ConnectionString;
        }
        
        _masterUow.Repository<Tenant>().Update(tenant);
        await _masterUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
