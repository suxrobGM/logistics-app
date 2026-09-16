using System.Collections.Concurrent;
using System.Reflection;
using Fluid;
using Logistics.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Communications.Email;

internal sealed class FluidEmailTemplateService : IEmailTemplateService
{
    private readonly Assembly _assembly;
    private readonly ILogger<FluidEmailTemplateService> _logger;
    private readonly TemplateOptions _options;
    private readonly FluidParser _parser = new();
    private readonly ConcurrentDictionary<string, IFluidTemplate> _templateCache = new();
    private readonly string _templateNamespace;

    public FluidEmailTemplateService(ILogger<FluidEmailTemplateService> logger)
    {
        _logger = logger;
        _assembly = typeof(FluidEmailTemplateService).Assembly;
        _templateNamespace = $"{_assembly.GetName().Name}.Email.Templates";

        _options = new TemplateOptions();
        RegisterEmailModels();
    }

    public async Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class
    {
        ArgumentException.ThrowIfNullOrEmpty(templateName);
        ArgumentNullException.ThrowIfNull(model);

        try
        {
            var template = await GetOrLoadTemplateAsync(templateName);
            var context = new TemplateContext(model, _options);
            return await template.RenderAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render email template '{TemplateName}'", templateName);
            throw new InvalidOperationException($"Failed to render email template '{templateName}'.", ex);
        }
    }

    private void RegisterEmailModels()
    {
        const string emailModelNamespace = "Logistics.Application.Abstractions.Email.Models";
        var emailModelAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Logistics.Application.Abstractions")
            ?? Assembly.Load("Logistics.Application.Abstractions");

        var emailModelTypes = emailModelAssembly.GetTypes()
            .Where(t => t.Namespace == emailModelNamespace && t is { IsClass: true, IsAbstract: false });

        foreach (var type in emailModelTypes)
        {
            _options.MemberAccessStrategy.Register(type);
        }
    }

    private async Task<IFluidTemplate> GetOrLoadTemplateAsync(string templateName)
    {
        if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var templateContent = await LoadTemplateContentAsync(templateName);

        if (!_parser.TryParse(templateContent, out var template, out var error))
        {
            throw new InvalidOperationException($"Failed to parse template '{templateName}': {error}");
        }

        _templateCache.TryAdd(templateName, template);
        return template;
    }

    private async Task<string> LoadTemplateContentAsync(string templateName)
    {
        var resourceName = $"{_templateNamespace}.{templateName}.liquid";
        await using var stream = _assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            var availableResources = _assembly.GetManifestResourceNames();
            _logger.LogWarning(
                "Template '{TemplateName}' not found. Looking for: {ResourceName}. Available: {AvailableResources}",
                templateName, resourceName, string.Join(", ", availableResources));
            throw new FileNotFoundException($"Email template '{templateName}' not found.", resourceName);
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
