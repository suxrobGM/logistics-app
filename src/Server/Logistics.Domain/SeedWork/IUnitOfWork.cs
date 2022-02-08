namespace Logistics.Domain;

/// <summary>
/// Unit of Work pattern
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> CommitAsync();
}