namespace Logistics.Application.Modules.Compliance.Dvir;

public interface IDvirReportAccess : IApplicationService
{
    /// <summary>
    /// Drivers hold <c>Dvir.Manage</c> to file their own, which alone would let any of them sign a
    /// compliance record as a colleague. <c>Dvir.Review</c> is what buys acting on someone else's.
    /// </summary>
    Task<bool> CanFileForAsync(Guid driverId, CancellationToken ct);
}
