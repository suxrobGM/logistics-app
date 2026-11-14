using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Logistics.API.Serialization;

/// <summary>
/// Names query parameters and form fields in camelCase, like JSON bodies. Model binding ignores case, so the wire accepts them.
/// Also drops query parameters that repeat a route value the controller assigns, such as <c>employeeId</c>.
/// </summary>
internal sealed class ParameterNameTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        RenameQueryParameters(operation);

        var formSchemas = (operation.RequestBody?.Content ?? new Dictionary<string, IOpenApiMediaType>())
            .Where(c => c.Key.Contains("form", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value.Schema)
            .OfType<OpenApiSchema>()
            .Where(s => s.Properties is not null);

        foreach (var schema in formSchemas)
        {
            RenameFormFields(schema);
        }

        return Task.CompletedTask;
    }

    private static void RenameQueryParameters(OpenApiOperation operation)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        var routeNames = operation.Parameters
            .Where(p => p.In == ParameterLocation.Path)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Query).ToList())
        {
            if (routeNames.Contains(parameter.Name!))
            {
                operation.Parameters.Remove(parameter);
            }
            else if (parameter is OpenApiParameter concrete)
            {
                concrete.Name = ToCamelCase(concrete.Name!);
            }
        }
    }

    private static void RenameFormFields(OpenApiSchema schema)
    {
        var fields = schema.Properties!.ToList();
        schema.Properties!.Clear();
        foreach (var (name, fieldSchema) in fields)
        {
            schema.Properties[ToCamelCase(name)] = fieldSchema;
        }

        if (schema.Required is { Count: > 0 } required)
        {
            schema.Required = required.Select(ToCamelCase).ToHashSet();
        }
    }

    private static string ToCamelCase(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}
