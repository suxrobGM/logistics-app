using Logistics.Domain.Entities;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Data;
using Logistics.Infrastructure.EF.Exceptions;
using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Http;

namespace Logistics.Infrastructure.EF.Services;

internal class TenantService : ITenantService
{
    private readonly TenantDbContextOptions? _dbContextOptions;
    private readonly MasterDbContext _masterDbContext;
    private readonly HttpContext? _httpContext;
    private Tenant? _currentTenant;

    public TenantService(
        MasterDbContext masterDbContext, 
        TenantDbContextOptions? dbContextContextOptions = null,
        IHttpContextAccessor? contextAccessor = null)
    {
        _dbContextOptions = dbContextContextOptions;
        _httpContext = contextAccessor?.HttpContext;
        _masterDbContext = masterDbContext ?? throw new ArgumentNullException(nameof(masterDbContext));
    }

    public Tenant GetTenant()
    {
        if (_currentTenant is not null)
        {
            return _currentTenant;
        }

        if (_httpContext is null)
        {
            var defaultTenant = new Tenant
            {
                Name = "default",
                ConnectionString = _dbContextOptions?.ConnectionString ?? ConnectionStrings.LocalDefaultTenant,
            };

            _currentTenant = defaultTenant;
            return defaultTenant;
        }
        
        var tenantHeader = _httpContext.Request.Headers["X-Tenant"];
        var tenantSubDomain = ParseSubDomain(_httpContext.Request.Host);
        var tenantClaim = _httpContext.User.Claims.FirstOrDefault(i => i.Type == "tenant")?.Value;
        string tenantId;

        if (!string.IsNullOrEmpty(tenantHeader))
        {
            _currentTenant = FetchCurrentTenant(tenantHeader);
            tenantId = tenantHeader.ToString();
        }
        else if (!string.IsNullOrEmpty(tenantSubDomain))
        {
            _currentTenant = FetchCurrentTenant(tenantSubDomain);
            tenantId = tenantSubDomain;
        }
        else if (!string.IsNullOrEmpty(tenantClaim))
        {
            _currentTenant = FetchCurrentTenant(tenantClaim);
            tenantId = tenantClaim;
        }
        else
        {
            throw new InvalidTenantException("Specify tenant ID in request header with the key 'X-Tenant'");
        }

        if (_currentTenant is null)
        {
            throw new InvalidTenantException($"Could not find tenant with ID '{tenantId}'");
        }
        
        return _currentTenant;
    }

    public Tenant? SetTenantById(string tenantId)
    {
        var tenant = FetchCurrentTenant(tenantId);
        _currentTenant = tenant;
        return _currentTenant;
    }

    public void SetTenant(Tenant tenant)
    {
        _currentTenant = tenant;
    }

    private Tenant? FetchCurrentTenant(string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidTenantException("Tenant ID is a null, specify tenant ID in request header with the key 'X-Tenant'");
        }

        tenantId = tenantId.Trim().ToLower();
        var tenant = _masterDbContext.Set<Tenant>().FirstOrDefault(i => i.Id == tenantId || i.Name == tenantId);
        return tenant;
    }

    private static string ParseSubDomain(HostString hostString)
    {
        var subDomain = string.Empty;
        var domains = hostString.Host.Split('.');

        if (domains.Length <= 2)
        {
            return subDomain;
        }

        subDomain = domains[0];
        return subDomain.ToLower();
    }
}
