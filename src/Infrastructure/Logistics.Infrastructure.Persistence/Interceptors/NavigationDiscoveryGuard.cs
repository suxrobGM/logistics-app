using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Logistics.Infrastructure.Persistence.Interceptors;

/// <summary>
///     Ids are pre-generated in the <c>Entity</c> base, so a new entity reaching the tracker only
///     via a parent's navigation is assumed to exist and saved as an UPDATE affecting 0 rows
///     (a misleading <see cref="DbUpdateConcurrencyException" />). This throws a named error
///     instead. Only fires for entities that start tracking as Modified during change detection -
///     exactly navigation discovery; explicit Update()/Attach() and query results stay allowed.
/// </summary>
public sealed class NavigationDiscoveryGuard
{
    private int detectingChanges;

    private NavigationDiscoveryGuard(ChangeTracker changeTracker)
    {
        changeTracker.DetectingAllChanges += (_, _) => detectingChanges++;
        changeTracker.DetectedAllChanges += (_, _) => detectingChanges--;
        changeTracker.DetectingEntityChanges += (_, _) => detectingChanges++;
        changeTracker.DetectedEntityChanges += (_, _) => detectingChanges--;
        changeTracker.Tracked += OnTracked;
    }

    public static void Attach(ChangeTracker changeTracker) => _ = new NavigationDiscoveryGuard(changeTracker);

    private void OnTracked(object? sender, EntityTrackedEventArgs e)
    {
        if (detectingChanges > 0 && !e.FromQuery && e.Entry.State == EntityState.Modified)
        {
            throw new InvalidOperationException(
                $"A new '{e.Entry.Metadata.ClrType.Name}' entity was discovered via a navigation property " +
                "and would be saved as an UPDATE affecting 0 rows, because its pre-generated id makes EF " +
                "assume it already exists. Register new entities explicitly with repository.AddAsync " +
                "before SaveChanges (adding them to the parent's collection alone is not enough).");
        }
    }
}
