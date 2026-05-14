using System.Reflection;
using FluentValidation;
using Logistics.Application.Behaviours;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Services;
using Logistics.Application.Services.Privacy;
using Logistics.Application.Services.Tax;
using Logistics.Application.Services.Tracking;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application;

public static class Registrar
{
    public static IServiceCollection AddApplicationTaxServices(this IServiceCollection services)
    {
        services.AddScoped<IInvoiceTaxApplier, InvoiceTaxApplier>();
        return services;
    }

    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(FeatureCheckBehaviour<,>));
            // Innermost: only binds to ICommand<T> via its generic constraint, so queries skip it.
            cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
        });

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITruckGeolocationUpdater, TruckGeolocationUpdater>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILoadService, LoadService>();
        services.AddScoped<IMaintenanceReminderService, MaintenanceReminderService>();
        services.AddScoped<ILicenseExpiryReminderService, LicenseExpiryReminderService>();
        services.AddScoped<IDispatchEligibilityService, DispatchEligibilityService>();
        services.AddScoped<IDataExportProcessingService, DataExportProcessingService>();
        services.AddScoped<IDataDeletionProcessingService, DataDeletionProcessingService>();
        services.AddScoped<IDataRetentionService, DataRetentionService>();
        services.AddScoped<IDataExportExpiryService, DataExportExpiryService>();
        services.AddApplicationTaxServices();
        return services;
    }
}
