using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Modules.Compliance.Safety.Services;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Behaviours;
using Logistics.Mediator;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Services;

public class ApplicationLayerRegistrationTests
{
    /// <summary>
    /// Builds the whole layer with validation on, so a service missing a registration or a
    /// singleton holding a scoped service fails here. Dependencies the infrastructure layer
    /// provides are stubbed: the units of work as scoped, like the real ones, the rest as
    /// singletons so they cannot raise a scope error of their own.
    /// </summary>
    [Fact]
    public void AddApplicationLayer_EveryServiceResolvesWithValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationLayer();
        services.AddScoped(_ => Substitute.For<IMasterUnitOfWork>());
        services.AddScoped(_ => Substitute.For<ITenantUnitOfWork>());
        // Identity registers these in the hosts; validation checks a factory exists, never calls it.
        services.AddScoped<UserManager<User>>(_ => throw new NotSupportedException());
        services.AddScoped<RoleManager<AppRole>>(_ => throw new NotSupportedException());
        StubInfrastructureDependencies(services);

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Theory]
    [InlineData(typeof(ILoadService))]
    [InlineData(typeof(IPayrollService))]
    [InlineData(typeof(IUserService))]
    [InlineData(typeof(IInvoiceTaxApplier))]
    public void AddApplicationLayer_RegistersAsScoped(Type serviceType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationLayer();

        var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    /// <summary>
    ///     The DbMigrator seeds payroll through this slice, so it must resolve from the units of work
    ///     and logging alone, with nothing else from the Application layer.
    /// </summary>
    [Fact]
    public void AddApplicationPayrollServices_ResolvesPayrollServiceFromUnitsOfWorkAlone()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => Substitute.For<IMasterUnitOfWork>());
        services.AddScoped(_ => Substitute.For<ITenantUnitOfWork>());
        services.AddApplicationPayrollServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPayrollService>());
    }

    /// <summary>
    ///     Registration order is pipeline order, so this pins which behaviour wraps which.
    /// </summary>
    [Fact]
    public void AddApplicationLayer_RegistersBehavioursOutermostFirst()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationLayer();

        var behaviours = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType!.Name)
            .ToArray();

        Assert.Equal(["ValidationBehaviour`2", "FeatureCheckBehaviour`2"], behaviours);
    }

    /// <summary>
    /// Registers a substitute for every interface a registered service's constructor asks for but
    /// nothing registers - unless the Application assembly implements it, since then a missing
    /// registration is the bug this test is for. Open generic services are skipped: their
    /// constructors are only checked when a closed type is requested.
    /// </summary>
    private static void StubInfrastructureDependencies(IServiceCollection services)
    {
        var applicationTypes = typeof(Registrar).Assembly.GetTypes();
        var implementations = services
            .Select(d => d.ImplementationType)
            .OfType<Type>()
            .Where(t => !t.IsGenericTypeDefinition)
            .Distinct();

        var missing = implementations
            .SelectMany(t => t.GetConstructors().SelectMany(c => c.GetParameters()))
            .Select(p => p.ParameterType)
            .Where(t => t.IsInterface && !IsRegistered(services, t))
            .Where(t => !applicationTypes.Any(a => a is { IsClass: true, IsAbstract: false } && a.IsAssignableTo(t)))
            .Distinct()
            .ToList();

        foreach (var type in missing)
        {
            services.AddSingleton(type, Substitute.For([type], []));
        }
    }

    private static bool IsRegistered(IServiceCollection services, Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return true;
        }

        var openType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
        return services.Any(d => d.ServiceType == type || d.ServiceType == openType);
    }
}
