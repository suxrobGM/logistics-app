using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class UpdateCustomerHandler(ITenantUnitOfWork tenantUow)
    : UpdateTenantEntityHandler<UpdateCustomerCommand, Customer>(tenantUow)
{
    protected override void ApplyChanges(UpdateCustomerCommand req, Customer customerEntity)
    {
        customerEntity.Name = PropertyUpdater.UpdateIfChanged(req.Name, customerEntity.Name);
        customerEntity.Email = PropertyUpdater.UpdateIfChanged(req.Email, customerEntity.Email);
        customerEntity.Phone = PropertyUpdater.UpdateIfChanged(req.Phone, customerEntity.Phone);
        customerEntity.Address = PropertyUpdater.UpdateIfChanged(req.Address, customerEntity.Address);
        customerEntity.Status = PropertyUpdater.UpdateIfChanged(req.Status, customerEntity.Status);
        customerEntity.Notes = PropertyUpdater.UpdateIfChanged(req.Notes, customerEntity.Notes);
        customerEntity.TaxId = PropertyUpdater.UpdateIfChanged(req.TaxId, customerEntity.TaxId);
        customerEntity.IsVatExempt = PropertyUpdater.UpdateIfChanged(req.IsVatExempt, customerEntity.IsVatExempt);
    }
}
