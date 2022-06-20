using System.Linq.Expressions;

namespace Logistics.Domain.Services;

public interface ITenantManager
{
    Task<Employee?> GetEmployeeAsync(string tenantId, Expression<Func<Employee, bool>> predicate);
}