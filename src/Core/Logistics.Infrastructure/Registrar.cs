using Logistics.Application.Services;
using Logistics.Application.Services.Geocoding;
using Logistics.Application.Services.PdfImport;
using Logistics.Infrastructure.Builder;
using Logistics.Infrastructure.Extensions;
using Logistics.Infrastructure.Interceptors;
using Logistics.Infrastructure.Options;
using Logistics.Infrastructure.Services;
using Logistics.Infrastructure.Services.Email;
using Logistics.Infrastructure.Services.Trip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.Configure<MapboxOptions>(configuration.GetSection("Mapbox"));
        services.AddHttpClient<MapboxMatrixClient>();

        services.AddScoped<HeuristicTripOptimizer>();
        services.AddScoped<MapboxMatrixTripOptimizer>();
        services.AddScoped<ITripOptimizer, CompositeTripOptimizer>();

        // PDF Import services
        services.AddScoped<IPdfDataExtractor, TemplateBasedDataExtractor>();

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
        services.AddScoped<DemoLoadBoardService>();
        services.AddScoped<ILoadBoardProviderFactory, LoadBoardProviderFactory>();

        // VIN Decoder Service
        services.AddHttpClient<IVinDecoderService, NhtsaVinDecoderService>();

        // Email Services
        var smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>();
        if (smtpOptions is not null)
        {
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<IEmailTemplateService, FluidEmailTemplateService>();
        }

        var googleRecaptchaOptions = configuration.GetSection("GoogleRecaptcha").Get<GoogleRecaptchaOptions>();
        if (googleRecaptchaOptions is not null)
        {
            services.Configure<GoogleRecaptchaOptions>(options =>
            {
                options.SecretKey = googleRecaptchaOptions.SecretKey;
                options.SiteKey = googleRecaptchaOptions.SiteKey;
            });
            services.AddSingleton<ICaptchaService, GoogleRecaptchaService>();
        }

        var stripeOptions = configuration.GetSection("Stripe").Get<StripeOptions>();
        if (stripeOptions is not null)
        {
            services.Configure<StripeOptions>(options =>
            {
                options.PublishableKey = stripeOptions.PublishableKey;
                options.SecretKey = stripeOptions.SecretKey;
                options.WebhookSecret = stripeOptions.WebhookSecret;
            });
            services.AddSingleton<IStripeService, StripeService>();
        }

        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        return new InfrastructureBuilder(services, configuration);
    }
}
