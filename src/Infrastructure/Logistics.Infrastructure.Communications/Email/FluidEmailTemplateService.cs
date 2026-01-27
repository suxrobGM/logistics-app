using System.Collections.Concurrent;
using System.Reflection;
using Fluid;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Communications.Email;

internal sealed class FluidEmailTemplateService : IEmailTemplateService
{
    private readonly Assembly assembly;
    private readonly ILogger<FluidEmailTemplateService> logger;
    private readonly TemplateOptions options;
    private readonly FluidParser parser = new();
    private readonly ConcurrentDictionary<string, IFluidTemplate> templateCache = new();
    private readonly string templateNamespace;

    public FluidEmailTemplateService(ILogger<FluidEmailTemplateService> logger)
    {
        this.logger = logger;
        assembly = typeof(FluidEmailTemplateService).Assembly;
        templateNamespace = $"{assembly.GetName().Name}.Email.Templates";

        options = new TemplateOptions();
        options.MemberAccessStrategy.Register<InvitationEmailModel>();
        options.MemberAccessStrategy.Register<TrackingLinkEmailModel>();
        options.MemberAccessStrategy.Register<InvoiceEmailModel>();
        options.MemberAccessStrategy.Register<InvoiceLineItemEmailModel>();
    }

    public async Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNull(model);

        try
        {
            var template = await GetOrLoadTemplateAsync(templateName);
            var context = new TemplateContext(model, options);
            return await template.RenderAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to render email template '{TemplateName}'", templateName);
            throw new InvalidOperationException($"Failed to render email template '{templateName}'.", ex);
        }
    }

    private async Task<IFluidTemplate> GetOrLoadTemplateAsync(string templateName)
    {
        if (templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var templateContent = await LoadTemplateContentAsync(templateName);

        if (!parser.TryParse(templateContent, out var template, out var error))
        {
            throw new InvalidOperationException($"Failed to parse template '{templateName}': {error}");
        }

        templateCache.TryAdd(templateName, template);
        return template;
    }

    private async Task<string> LoadTemplateContentAsync(string templateName)
    {
        var resourceName = $"{templateNamespace}.{templateName}.liquid";
        await using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            var availableResources = assembly.GetManifestResourceNames();
            logger.LogWarning(
                "Template '{TemplateName}' not found. Looking for: {ResourceName}. Available: {AvailableResources}",
                templateName, resourceName, string.Join(", ", availableResources));
            throw new FileNotFoundException($"Email template '{templateName}' not found.", resourceName);
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
