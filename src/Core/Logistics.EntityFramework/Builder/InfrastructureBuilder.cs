using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.EntityFramework.Builder;

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

    public IInfrastructureBuilder ConfigureMainDatabase(Action<MainDbContextOptions> configure)
    {
        var options = new MainDbContextOptions();
        configure(options);

        var serviceDesc = new ServiceDescriptor(typeof(MainDbContextOptions), options);

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