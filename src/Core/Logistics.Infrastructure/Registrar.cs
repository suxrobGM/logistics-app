using Logistics.Application.Services;
using Logistics.Application.Services.Geocoding;
using Logistics.Application.Services.Pdf;
using Logistics.Application.Services.PdfImport;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Builder;
using Logistics.Infrastructure.Extensions;
using Logistics.Infrastructure.Interceptors;
using Logistics.Infrastructure.Options;
using Logistics.Infrastructure.Services;
using Logistics.Infrastructure.Services.Dat;
using Logistics.Infrastructure.Services.Email;
using Logistics.Infrastructure.Services.OneTwo3;
using Logistics.Infrastructure.Services.Pdf;
using Logistics.Infrastructure.Services.Trip;
using Logistics.Infrastructure.Services.Truckstop;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MapboxGeocodingService = Logistics.Infrastructure.Services.Geocoding.MapboxGeocodingService;
using TemplateBasedDataExtractor = Logistics.Infrastructure.Services.PdfImport.TemplateBasedDataExtractor;

namespace Logistics.Infrastructure;

public static class Registrar
{
    /// <summary>
    ///     Add the infrastructure layer to the service collection.
    ///     It also adds the domain layer and the necessary services.
    ///     To add master and tenant databases, use the AddMasterDatabase and AddTenantDatabase methods.
    ///     To add identity, use the AddIdentity method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The infrastructure builder.</returns>
    public static IInfrastructureBuilder AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<DispatchDomainEventsInterceptor>());

        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        // Blob Storage (Azure or File based on configuration)
        services.AddFileBlobStorage(configuration);

        services.Configure<MapboxOptions>(configuration.GetSection(MapboxOptions.SectionName));
        services.Configure<CustomerPortalOptions>(configuration.GetSection(CustomerPortalOptions.SectionName));
        services.Configure<IdentityServerOptions>(configuration.GetSection(IdentityServerOptions.SectionName));
        services.AddHttpClient<MapboxMatrixClient>();

        // Current user service (only for web apps with IHttpContextAccessor)
        services.TryAddScoped<ICurrentUserService>(sp =>
        {
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            return httpContextAccessor is not null
                ? new CurrentUserService(httpContextAccessor)
                : new NoopCurrentUserService();
        });

        services.AddScoped<HeuristicTripOptimizer>();
        services.AddScoped<MapboxMatrixTripOptimizer>();
        services.AddScoped<ITripOptimizer, CompositeTripOptimizer>();

        // PDF services
        services.AddScoped<IPdfDataExtractor, TemplateBasedDataExtractor>();
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();

        // Geocoding services
        services.AddHttpClient<IGeocodingService, MapboxGeocodingService>();

        // ELD Provider Services
        services.Configure<EldOptions>(configuration.GetSection("Eld"));
        services.AddHttpClient<SamsaraEldService>();
        services.AddHttpClient<MotiveEldService>();
        services.AddScoped<DemoEldService>();
        services.AddScoped<IEldProviderFactory, EldProviderFactory>();

        // Load Board Provider Services
        services.Configure<LoadBoardOptions>(configuration.GetSection("LoadBoard"));
        services.AddHttpClient<DatLoadBoardService>();
        services.AddHttpClient<TruckstopLoadBoardService>();
        services.AddHttpClient<OneTwo3LoadBoardService>();
        services.AddScoped<DemoLoadBoardService>();
        services.AddScoped<ILoadBoardProviderFactory, LoadBoardProviderFactory>();

        // VIN Decoder Service
        services.AddHttpClient<IVinDecoderService, NhtsaVinDecoderService>();

        // Email Services
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();

        // Google Recaptcha
        services.Configure<GoogleRecaptchaOptions>(configuration.GetSection(GoogleRecaptchaOptions.SectionName));
        services.AddSingleton<ICaptchaService, GoogleRecaptchaService>();

        // Stripe Payment Services
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.AddSingleton<IStripeService, StripeService>();
        services.AddSingleton<IStripeConnectService, StripeConnectService>();

        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        return new InfrastructureBuilder(services, configuration);
    }
}
