using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.EF.Builder;

internal class InfrastructureBuilder : ServiceCollection, IInfrastructureBuilder
{
    private readonly IdentityBuilder _identityBuilder;
    
    internal InfrastructureBuilder(IdentityBuilder identityBuilder)
    {
        _identityBuilder = identityBuilder;
    }
    
    public IInfrastructureBuilder ConfigureIdentity(Action<IdentityBuilder> configure)
    {
        configure(_identityBuilder);
        return this;
    }

    public IInfrastructureBuilder ConfigureMainDatabase(Action<MasterDbContextOptions> configure)
    {
        var options = new MasterDbContextOptions();
        configure(options);

        var serviceDesc = new ServiceDescriptor(typeof(MasterDbContextOptions), options);

        if (Contains(serviceDesc))
        {
            Remove(serviceDesc);
        }
        
        this.AddSingleton(options);
        return this;
    }

    public IInfrastructureBuilder ConfigureTenantDatabase(Action<TenantDbContextOptions> configure)
    {
        var options = new TenantDbContextOptions();
        configure(options);

        var serviceDesc = new ServiceDescriptor(typeof(TenantDbContextOptions), options);

        if (Contains(serviceDesc))
        {
            Remove(serviceDesc);
        }
        
        this.AddSingleton(options);
        return this;
    }
}