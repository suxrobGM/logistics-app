using Logistics.Domain.Services;
using Microsoft.AspNetCore.Http;

namespace Logistics.EntityFramework.Services;

internal class TenantService : ITenantService
{
    private readonly IMainRepository<Tenant> _repository;
    private readonly HttpContext _httpContext;
    private readonly Tenant _currentTenant;

    public TenantService(IMainRepository<Tenant> repository, IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext;
        _repository = repository;

        if (_httpContext != null && _httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
        {
            _currentTenant = GetCurrentTenant(tenantId);
        }
        else
        {
            throw new InvalidOperationException("Invalid Tenant!");
        }
    }

    private Tenant GetCurrentTenant(string tenantId)
    {
        var tenant = _repository.GetQuery().FirstOrDefault(i => i.Id == tenantId);
        if (tenant == null) 
            throw new InvalidOperationException("Invalid Tenant!");

        return tenant;
    }

    public string GetConnectionString()
    {
        var connectionString = _currentTenant.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Invalid tenant's connection string");

        return connectionString;
    }

    public Tenant GetTenant()
    {
        return _currentTenant;
    }
}
