using Logistics.Domain.Entities;

namespace Logistics.Application.Services;

/// <summary>
///     Interface for the Load service that provides methods to manage loads in the logistics system.
/// </summary>
public interface ILoadService
{
    /// <summary>
    ///     Creates a new load with the specified parameters.
    ///     It creates a new load, stores it in the database (if the saveChanges flag is true), and returns the created load
    ///     entity.
    ///     If the load creation fails, it throws an exception.
    /// </summary>
    /// <param name="parameters">Required parameters for creating a load</param>
    /// <param name="saveChanges">Optional parameter to indicate whether to save changes immediately, default is true</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Load entity</returns>
    /// <exception cref="InvalidOperationException">Thrown when the load creation fails</exception>
    Task<Load> CreateLoadAsync(CreateLoadParameters parameters, bool saveChanges = true,
        CancellationToken ct = default);

    /// <summary>
    ///     Creates multiple loads with the specified parameters.
    ///     It creates new loads, stores them in the database (if the saveChanges flag is true), and returns the created load
    ///     entities.
    ///     If any load creation fails, it throws an exception.
    /// </summary>
    /// <param name="parameters">Enumerable of required parameters for creating loads</param>
    /// <param name="saveChanges">Optional parameter to indicate whether to save changes immediately, default is true</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown when any load creation fails</exception>
    Task<IReadOnlyCollection<Load>> CreateLoadsAsync(IEnumerable<CreateLoadParameters> parameters,
        bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    ///     Deletes the specified load.
    /// </summary>
    /// <param name="loadId">Load ID to delete</param>
    Task DeleteLoadAsync(Guid loadId);
}
