namespace Logistics.Application.Utilities;

/// <summary>
/// A utility class to update properties based on the source value.
/// </summary>
public static class PropertyUpdater
{
    /// <summary>
    /// Update the property if the source is not null or empty and is different from the current value.
    /// </summary>
    /// <param name="newValue">The source value to update from.</param>
    /// <param name="previousValue">The current value from the database context.</param>
    /// <param name="transform">The transformation function to apply to the source value.</param>
    /// <returns>The updated value.</returns>
    public static string UpdateIfChanged(string? newValue, string previousValue, Func<string, string>? transform = null)
    {
        if (!string.IsNullOrEmpty(newValue) && newValue != previousValue)
        {
            return transform != null ? transform(newValue) : newValue;
        }
        return previousValue;
    }
    
    /// <summary>
    /// Update the property if the source is not null and is different from the current value.
    /// For non-string reference properties (objects).
    /// </summary>
    /// <param name="newValue">The source value to update from.</param>
    /// <param name="previousValue">The current value from the database context.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>The updated value.</returns>
    public static T UpdateIfChanged<T>(T? newValue, T previousValue) where T : class?
    {
        if (newValue != null && !EqualityComparer<T>.Default.Equals(newValue, previousValue))
        {
            return newValue;
        }
        return previousValue;
    }
    
    /// <summary>
    /// Update the property if the source is not null and is different from the current value.
    /// For non-string value properties (primitives, structs).
    /// </summary>
    /// <param name="newValue">The source value to update from.</param>
    /// <param name="previousValue">The current value from the database context.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>The updated value.</returns>
    public static T UpdateIfChanged<T>(T? newValue, T previousValue) where T : struct
    {
        if (newValue.HasValue && !EqualityComparer<T>.Default.Equals(newValue.Value, previousValue))
        {
            return newValue.Value;
        }
        return previousValue;
    }
}