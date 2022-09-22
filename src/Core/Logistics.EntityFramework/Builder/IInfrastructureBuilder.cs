using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.EntityFramework.Builder;

public interface IInfrastructureBuilder : IServiceCollection
{
    IInfrastructureBuilder ConfigureIdentity(Action<IdentityBuilder> configure);
    IInfrastructureBuilder ConfigureMainDatabase(Action<MainDbContextOptions> configure);
    IInfrastructureBuilder ConfigureTenantDatabase(Action<TenantDbContextOptions> configure);
}