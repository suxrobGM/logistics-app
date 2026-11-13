using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Logistics.API.Serialization;

/// <summary>
/// Types <see cref="DateTime"/> as a date-time string. <see cref="Converters.Iso8601UtcDateTimeConverter"/>
/// is a custom converter, so the schema exporter can't infer the type and emits only the format.
/// </summary>
internal sealed class DateTimeSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (type == typeof(DateTime))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "date-time";
        }
        else if (type == typeof(DateTime?))
        {
            schema.Type = JsonSchemaType.String | JsonSchemaType.Null;
            schema.Format = "date-time";
        }

        return Task.CompletedTask;
    }
}
