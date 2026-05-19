using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Modules.Compliance.Safety.Services;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Abstractions.Realtime;
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
