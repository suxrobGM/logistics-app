using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Logistics.API.Serialization;

public sealed class UtcDateTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(DateTime) ? new UtcDateTimeModelBinder() : null;
    }
}
