using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Logistics.API.ModelBinders;

/// <summary>
///     Model binder that converts snake_case strings to PascalCase enum values.
///     This allows query string parameters like ?Status=picked_up to bind to LoadStatus.PickedUp.
/// </summary>
public class SnakeCaseEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelType = bindingContext.ModelType;
        var isNullable = Nullable.GetUnderlyingType(modelType) != null;
        var enumType = isNullable ? Nullable.GetUnderlyingType(modelType)! : modelType;

        if (!enumType.IsEnum)
        {
            return Task.CompletedTask;
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            if (isNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            return Task.CompletedTask;
        }

        if (Enum.TryParse(enumType, value.Replace("_", string.Empty), ignoreCase: true, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"The value '{value}' is not valid for {bindingContext.ModelName}.");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
///     Model binder provider that provides SnakeCaseEnumModelBinder for enum types.
/// </summary>
public class SnakeCaseEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;
        var underlyingType = Nullable.GetUnderlyingType(modelType) ?? modelType;

        if (underlyingType.IsEnum)
        {
            return new SnakeCaseEnumModelBinder();
        }

        return null;
    }
}
