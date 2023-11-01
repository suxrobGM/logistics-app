using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.EF.Builder;

public interface IInfrastructureBuilder : IServiceCollection
{
    IInfrastructureBuilder ConfigureIdentity(Action<IdentityBuilder> configure);
    IInfrastructureBuilder ConfigureMainDatabase(Action<MasterDbContextOptions> configure);
    IInfrastructureBuilder ConfigureTenantDatabase(Action<TenantDbContextOptions> configure);
}