using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Logistics.API.Serialization;

/// <summary>
/// Names query parameters in camelCase, like JSON bodies. Model binding ignores case, so the wire accepts them.
/// Also drops query parameters that repeat a route value the controller assigns, such as <c>employeeId</c>.
/// </summary>
internal sealed class QueryParameterNameTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Parameters is null)
        {
            return Task.CompletedTask;
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
                concrete.Name = JsonNamingPolicy.CamelCase.ConvertName(concrete.Name!);
            }
        }

        return Task.CompletedTask;
    }
}
