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
        _httpContext = contextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor));
        _repository = repository;
        var subDomain = GetSubDomain(_httpContext.Request.Host);

        if (_httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
        {
            _currentTenant = GetCurrentTenant(tenantId);
        }
        else if (!string.IsNullOrEmpty(subDomain))
        {
            _currentTenant = GetCurrentTenant(subDomain);
        }
        else
        {
            throw new InvalidOperationException("Invalid Tenant!");
        }
    }

    private Tenant GetCurrentTenant(string tenantId)
    {
        var tenant = _repository.GetQuery().FirstOrDefault(i => i.Id == tenantId || i.Name == tenantId);
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

    private string GetSubDomain(HostString hostString)
    {
        var subDomain = string.Empty;
        var domains = hostString.Host.Split('.');

        if (domains.Length <= 2)
            return subDomain;

        subDomain = domains[0];
        return subDomain;
    }
}
