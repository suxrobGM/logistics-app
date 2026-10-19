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
using Logistics.Domain.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Services;

public class ApplicationLayerRegistrationTests
{
    [Theory]
    [InlineData(typeof(ILoadService))]
    [InlineData(typeof(IPayrollService))]
    [InlineData(typeof(IUserService))]
    [InlineData(typeof(ILicenseExpiryReminderService))]
    [InlineData(typeof(IInvitationExpiryService))]
    [InlineData(typeof(IDispatchEligibilityService))]
    [InlineData(typeof(ITruckGeolocationUpdater))]
    [InlineData(typeof(IDataExportProcessingService))]
    [InlineData(typeof(IDataDeletionProcessingService))]
    [InlineData(typeof(IDataRetentionService))]
    [InlineData(typeof(IDataExportExpiryService))]
    [InlineData(typeof(IInvoiceTaxApplier))]
    [InlineData(typeof(IProductLicenseService))]
    [InlineData(typeof(IProductLicenseHeartbeatService))]
    public void AddApplicationLayer_RegistersService(Type serviceType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationLayer();

        var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);

        Assert.NotNull(descriptor);
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
}
