namespace Logistics.Application.Utilities;

/// <summary>
/// A utility class to update properties based on the source value.
/// </summary>
public static class PropertyUpdater
{
    /// <summary>
    /// Update the property if the source is not null or empty and is different from the current value.
    /// </summary>
    /// <param name="source">The source value to update from.</param>
    /// <param name="current"> The current value to update.</param>
    /// <param name="transform">The transformation function to apply to the source value.</param>
    /// <returns>The updated value.</returns>
    public static string UpdateIfChanged(string? source, string current, Func<string, string>? transform = null)
    {
        if (!string.IsNullOrEmpty(source) && source != current)
        {
            return transform != null ? transform(source) : source;
        }
        return current;
    }
    
    /// <summary>
    /// Update the property if the source is not null and is different from the current value.
    /// For non-string properties (objects, primitives)
    /// </summary>
    /// <param name="source">The source value to update from.</param>
    /// <param name="current"> The current value to update.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>The updated value.</returns>
    public static T UpdateIfChanged<T>(T? source, T current) where T : class?
    {
        if (source != null && !EqualityComparer<T>.Default.Equals(source, current))
        {
            return source;
        }
        return current;
    }
    
    /// <summary>
    /// Update the property if the source is not null and is different from the current value.
    /// </summary>
    /// <param name="source">The source value to update from.</param>
    /// <param name="current">The current value to update. Pass by reference.</param>
    /// <param name="transform">The transformation function to apply to the source value.</param>
    public static void UpdateIfChanged(string source, ref string current, Func<string, string>? transform = null)
    {
        current = UpdateIfChanged(source, current, transform);
    }
    
    /// <summary>
    /// Update the property if the source is not null and is different from the current value.
    /// For non-string properties (objects, primitives)
    /// </summary>
    /// <param name="source">The source value to update from.</param>
    /// <param name="current">The current value to update. Pass by reference.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    public static void UpdateIfChanged<T>(T? source, ref T current) where T : class
    {
        current = UpdateIfChanged(source, current);
    }
}