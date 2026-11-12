namespace Logistics.Application.Modules.Operations.Common.Services;

public static class DisplayNameMapExtensions
{
    extension(IReadOnlyDictionary<Guid, string> names)
    {
        /// <summary>The name for <paramref name="id" />, or null when the id is null or was not found.</summary>
        public string? Find(Guid? id) => id is { } key ? names.GetValueOrDefault(key) : null;
    }
}
