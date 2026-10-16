using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Operations.Trailers.Services;

/// <summary>
/// Hooks trailers to trucks. A trailer can be hooked to one truck at a time, which takes a lookup
/// across trucks, so that part of the rule lives here rather than on <see cref="Truck"/>.
/// </summary>
public interface ITrailerHookService : IApplicationService
{
    /// <summary>
    /// Hooks <paramref name="trailer"/> to <paramref name="truck"/>, or unhooks the current one when null.
    /// A trailer hooked to another truck is unhooked there first, which saves changes.
    /// </summary>
    /// <exception cref="InvalidOperationException">The trailer is not Active.</exception>
    Task HookAsync(Truck truck, Trailer? trailer, CancellationToken ct = default);

    /// <summary>
    /// Unhooks <paramref name="trailer"/> from the truck pulling it once it is no longer Active.
    /// </summary>
    Task UnhookUnlessActiveAsync(Trailer trailer, CancellationToken ct = default);
}
