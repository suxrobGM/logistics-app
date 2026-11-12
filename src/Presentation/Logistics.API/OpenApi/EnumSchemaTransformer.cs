using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Logistics.API.OpenApi;

/// <summary>
/// Fixes the shared enum schemas the generator emits: it leaves out <c>type: string</c>, and copies
/// <c>null</c> into the values from the first nullable property it meets. Nullable properties keep
/// their own null branch, so dropping it here loses nothing.
/// </summary>
internal sealed class EnumSchemaTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var schemas = document.Components?.Schemas?.Values ?? [];
        foreach (var schema in schemas.OfType<OpenApiSchema>())
        {
            if (schema.Enum is not { Count: > 0 })
            {
                continue;
            }

            for (var i = schema.Enum.Count - 1; i >= 0; i--)
            {
                if (schema.Enum[i] is null)
                {
                    schema.Enum.RemoveAt(i);
                }
            }

            schema.Type ??= JsonSchemaType.String;
        }

        return Task.CompletedTask;
    }
}
