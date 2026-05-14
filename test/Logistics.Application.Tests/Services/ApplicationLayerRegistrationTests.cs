using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Services;
using Logistics.Application.Services.Privacy;
using Logistics.Application.Services.Tax;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Logistics.Application.Tests.Services;

public class ApplicationLayerRegistrationTests
{
    [Theory]
    [InlineData(typeof(ILoadService))]
    [InlineData(typeof(IPayrollService))]
    [InlineData(typeof(IUserService))]
    [InlineData(typeof(IMaintenanceReminderService))]
    [InlineData(typeof(ILicenseExpiryReminderService))]
    [InlineData(typeof(IInvitationExpiryService))]
    [InlineData(typeof(IDispatchEligibilityService))]
    [InlineData(typeof(ITruckGeolocationUpdater))]
    [InlineData(typeof(IDataExportProcessingService))]
    [InlineData(typeof(IDataDeletionProcessingService))]
    [InlineData(typeof(IDataRetentionService))]
    [InlineData(typeof(IDataExportExpiryService))]
    [InlineData(typeof(IInvoiceTaxApplier))]
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
}
