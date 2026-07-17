using System.Text;

namespace Logistics.Application.Handlers;

/// <summary>
///     Produces human-readable entity names for standardized not-found messages.
/// </summary>
internal static class EntityName
{
    /// <summary>
    ///     Turns a PascalCase entity type name into a lowercase, space-separated phrase
    ///     (e.g. <c>CustomerUser</c> becomes <c>customer user</c>).
    /// </summary>
    public static string Humanize<TEntity>()
    {
        var name = typeof(TEntity).Name;
        var sb = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                sb.Append(' ');
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}
