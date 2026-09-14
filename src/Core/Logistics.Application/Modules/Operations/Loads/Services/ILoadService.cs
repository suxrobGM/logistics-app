using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>
///     Interface for the Load service that provides methods to manage loads in the logistics system.
/// </summary>
public interface ILoadService : IApplicationService
{
    /// <summary>
    ///     Creates a load and stores it when <paramref name="saveChanges" /> is true. Fails when a referenced
    ///     dispatcher, truck or customer is missing, or when a vehicle load needs a feature the tenant does not have.
    /// </summary>
    Task<Result<Load>> CreateLoadAsync(CreateLoadParameters parameters, bool saveChanges = true,
        CancellationToken ct = default);

    /// <summary>
    ///     Creates loads in one batch and returns them in the order given. Fails as a whole, for the same reasons as
    ///     <see cref="CreateLoadAsync" />.
    /// </summary>
    Task<Result<IReadOnlyList<Load>>> CreateLoadsAsync(IEnumerable<CreateLoadParameters> parameters,
        bool saveChanges = true, CancellationToken ct = default);

    /// <summary>
    ///     Deletes the specified load.
    /// </summary>
    /// <param name="loadId">Load ID to delete</param>
    Task DeleteLoadAsync(Guid loadId);
}
