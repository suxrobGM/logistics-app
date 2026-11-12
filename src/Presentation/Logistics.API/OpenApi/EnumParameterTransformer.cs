using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Logistics.API.OpenApi;

/// <summary>
/// Points enum query, route and form fields at the shared enum schema. The generator lists them by
/// C# name, but <see cref="ModelBinders.SnakeCaseEnumModelBinder"/> binds the snake_case values.
/// </summary>
internal sealed class EnumParameterTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var document = context.Document;
        if (document is null)
        {
            return;
        }

        var formSchema = operation.RequestBody?.Content?.Values.FirstOrDefault()?.Schema as OpenApiSchema;

        foreach (var description in context.Description.ParameterDescriptions)
        {
            var enumType = Nullable.GetUnderlyingType(description.Type) ?? description.Type;
            if (enumType is null || !enumType.IsEnum)
            {
                continue;
            }

            var schema = await context.GetOrCreateSchemaAsync(enumType, cancellationToken: cancellationToken);
            document.AddComponent(enumType.Name, schema);
            var reference = new OpenApiSchemaReference(enumType.Name, document);

            if (description.Source == BindingSource.Form)
            {
                if (formSchema?.Properties?.ContainsKey(description.Name) == true)
                {
                    formSchema.Properties[description.Name] = reference;
                }

                continue;
            }

            var parameter = operation.Parameters?.OfType<OpenApiParameter>()
                .FirstOrDefault(p => p.Name == description.Name);
            if (parameter is not null)
            {
                parameter.Schema = reference;
            }
        }
    }
}
