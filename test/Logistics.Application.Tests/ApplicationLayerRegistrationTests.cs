using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Common.Pipeline;
using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Modules.Compliance.Safety.Services;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests;

public class ApplicationLayerRegistrationTests
{
    /// <summary>Fails on a missing registration or a singleton holding a scoped service.</summary>
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

    /// <summary>The DbMigrator seeds payroll through this slice, so it must resolve from the units of work alone.</summary>
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

    /// <summary>Registration order is pipeline order, so this pins which behaviour wraps which.</summary>
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

    /// <summary>Substitutes unregistered constructor interfaces, except ones the Application assembly implements.</summary>
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

        // Singletons, so a stub cannot raise a scope error of its own.
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
