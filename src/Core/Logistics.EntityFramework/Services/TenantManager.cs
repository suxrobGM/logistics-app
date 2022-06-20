using System.Linq.Expressions;
using Logistics.Domain.Services;

namespace Logistics.EntityFramework.Services;

internal class TenantManager : ITenantManager
{
    private readonly IMainRepository<Tenant> _mainRepository;
    
    public TenantManager(IMainRepository<Tenant> mainRepository)
    {
        _mainRepository = mainRepository;
    }

    public async Task<Employee?> GetEmployeeAsync(string tenantId, Expression<Func<Employee, bool>> predicate)
    {
        var tenant = await _mainRepository.GetAsync(i =>
            i.Id.Equals(tenantId, StringComparison.InvariantCultureIgnoreCase) ||
            i.Name!.Equals(tenantId, StringComparison.InvariantCultureIgnoreCase));

        if (tenant == null)
        {
            throw new InvalidOperationException($"Could not find the specified tenant '{tenantId}'");
        }

        await using var tenantDbContext = new TenantDbContext(tenant.ConnectionString!);
        var employee = await tenantDbContext.Set<Employee>().FirstOrDefaultAsync(predicate);
        return employee;
    }
}