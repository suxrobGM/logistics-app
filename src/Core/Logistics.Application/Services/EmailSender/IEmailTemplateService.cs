namespace Logistics.Application.Services;

/// <summary>
/// Service for rendering email templates with dynamic data.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders an email template with the provided model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="templateName">The name of the template (without extension).</param>
    /// <param name="model">The model data to render into the template.</param>
    /// <returns>The rendered HTML content.</returns>
    Task<string> RenderAsync<TModel>(string templateName, TModel model) where TModel : class;
}
