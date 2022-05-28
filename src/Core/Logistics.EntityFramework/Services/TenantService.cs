using Microsoft.AspNetCore.Http;
using Logistics.Domain.Services;
using Logistics.Domain.Shared.Exceptions;

namespace Logistics.EntityFramework.Services;

internal class TenantService : ITenantService
{
    private readonly IMainRepository<Tenant> _repository;
    private readonly HttpContext _httpContext;
    private Tenant _currentTenant;

    public TenantService(IMainRepository<Tenant> repository, IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentTenant = null!;
    }

    public Tenant GetTenant()
    {
        if (_currentTenant != null)
        {
            return _currentTenant;
        }

        var subDomain = GetSubDomain(_httpContext.Request.Host);

        if (_httpContext.Request.Headers.TryGetValue("X-TenantId", out var tenantId))
        {
            _currentTenant = GetCurrentTenant(tenantId);
        }
        else if (!string.IsNullOrEmpty(subDomain))
        {
            _currentTenant = GetCurrentTenant(subDomain);
        }
        else
        {
            throw new InvalidTenantException("Specify tenant ID in request header with the key 'X-TenantId'");
        }
        return _currentTenant;
    }

    private Tenant GetCurrentTenant(string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidTenantException("Tenant ID is a null, specify tenant ID in request header with the key 'X-TenantId'");
        }

        var tenant = _repository.GetQuery().FirstOrDefault(i => i.Id == tenantId || i.Name == tenantId) ??
            throw new InvalidTenantException($"Could not found tenant with ID '{tenantId}'");
            
        return tenant;
    }

    public string GetConnectionString()
    {
        var connectionString = GetTenant().ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidTenantException("Invalid tenant's connection string");

        return connectionString;
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
