using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Logistics.API.Serialization;

/// <summary>Binds query and route dates as UTC; the default binder returns local time, which Npgsql rejects.</summary>
public sealed class UtcDateTimeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var value = valueResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) is not null)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }

            return Task.CompletedTask;
        }

        if (UtcDateParser.TryParse(value, out var utc))
        {
            bindingContext.Result = ModelBindingResult.Success(utc);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"The value '{value}' is not a valid ISO 8601 date for {bindingContext.ModelName}.");
        }

        return Task.CompletedTask;
    }
}
