namespace Logistics.Domain.Repositories;

/// <summary>
/// Apllication's UOW
/// </summary>
public interface IMainUnitOfWork
{
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> CommitAsync();
}