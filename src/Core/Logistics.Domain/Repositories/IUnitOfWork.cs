namespace Logistics.Domain.Repositories;

/// <summary>
/// Unit of work pattern
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> CommitAsync();
}