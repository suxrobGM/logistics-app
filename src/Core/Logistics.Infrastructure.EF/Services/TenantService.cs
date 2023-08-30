using Logistics.Infrastructure.EF.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Logistics.Infrastructure.EF.Services;

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

    public async Task<Tenant> GetTenantAsync()
    {
        if (_currentTenant is not null)
            return _currentTenant;

        if (_httpContext is null)
        {
            _currentTenant = new Tenant
            {
                ConnectionString = _dbOptions.ConnectionString,
                Name = "default"
            };

            return _currentTenant;
        }
        
        var tenantHeader = _httpContext.Request.Headers["X-Tenant"];
        var tenantSubDomain = ParseSubDomain(_httpContext.Request.Host);
        var tenantClaim = _httpContext.User.Claims.FirstOrDefault(i => i.Type == "tenant")?.Value;
        string tenantId;

        if (!string.IsNullOrEmpty(tenantHeader))
        {
            _currentTenant = await FetchCurrentTenantAsync(tenantHeader);
            tenantId = tenantHeader.ToString();
        }
        else if (!string.IsNullOrEmpty(tenantSubDomain))
        {
            _currentTenant = await FetchCurrentTenantAsync(tenantSubDomain);
            tenantId = tenantSubDomain;
        }
        else if (!string.IsNullOrEmpty(tenantClaim))
        {
            _currentTenant = await FetchCurrentTenantAsync(tenantClaim);
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

    public async Task<bool> SetTenantAsync(string tenantId)
    {
        var tenant = await FetchCurrentTenantAsync(tenantId);

        if (tenant is null)
            return false;

        _currentTenant = tenant;
        return true;
    }

    private async Task<Tenant?> FetchCurrentTenantAsync(string? tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidTenantException("Tenant ID is a null, specify tenant ID in request header with the key 'X-Tenant'");
        }

        tenantId = tenantId.Trim().ToLower();
        var tenant = await _mainRepository.Query<Tenant>()
            .FirstOrDefaultAsync(i => i.Id == tenantId || i.Name == tenantId);
        
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
