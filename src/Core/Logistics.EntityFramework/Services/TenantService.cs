using Microsoft.AspNetCore.Http;
using Logistics.Domain.Shared.Exceptions;

namespace Logistics.EntityFramework.Services;

internal class TenantService : ITenantService
{
    private readonly IMainRepository _mainRepository;
    private readonly TenantDbContextOptions _dbOptions;
    private readonly HttpContext? _httpContext;
    private Tenant? _currentTenant;

    public TenantService(
        TenantDbContextOptions contextOptions,
        IMainRepository repository, 
        IHttpContextAccessor? contextAccessor = null)
    {
        _httpContext = contextAccessor?.HttpContext;
        _dbOptions = contextOptions ?? throw new ArgumentNullException(nameof(contextOptions));
        _mainRepository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Tenant GetTenant()
    {
        if (_httpContext == null)
        {
            _currentTenant = new Tenant
            {
                ConnectionString = _dbOptions.ConnectionString,
                Name = "default"
            };

            return _currentTenant;
        }

        if (_currentTenant != null)
            return _currentTenant;
        
        var tenantHeader = _httpContext.Request.Headers["X-Tenant"];
        var tenantSubDomain = ParseSubDomain(_httpContext.Request.Host);
        var tenantClaim = _httpContext.User.Claims.FirstOrDefault(i => i.Type == "tenant")?.Value;

        if (!string.IsNullOrEmpty(tenantHeader))
        {
            _currentTenant = FetchCurrentTenant(tenantHeader);
        }
        else if (!string.IsNullOrEmpty(tenantSubDomain))
        {
            _currentTenant = FetchCurrentTenant(tenantSubDomain);
        }
        else if (!string.IsNullOrEmpty(tenantClaim))
        {
            _currentTenant = FetchCurrentTenant(tenantClaim);
        }
        else
        {
            throw new InvalidTenantException("Specify tenant ID in request header with the key 'X-Tenant'");
        }
        return _currentTenant;
    }
    
    public string GetConnectionString()
    {
        var connectionString = GetTenant().ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidTenantException("Invalid tenant's connection string");

        return connectionString;
    }

    private Tenant FetchCurrentTenant(string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidTenantException("Tenant ID is a null, specify tenant ID in request header with the key 'X-Tenant'");
        }

        tenantId = tenantId.Trim().ToLower();
        var tenant = _mainRepository.Query<Tenant>().FirstOrDefault(i => i.Id == tenantId || i.Name == tenantId) ??
            throw new InvalidTenantException($"Could not find tenant with ID '{tenantId}'");
        
        return tenant;
    }

    private static string ParseSubDomain(HostString hostString)
    {
        var subDomain = string.Empty;
        var domains = hostString.Host.Split('.');

        if (domains.Length <= 2)
            return subDomain;

        subDomain = domains[0];
        return subDomain.ToLower();
    }
}
