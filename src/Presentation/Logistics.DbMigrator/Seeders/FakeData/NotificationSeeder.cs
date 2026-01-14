using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds test notifications.
/// </summary>
internal class NotificationSeeder(ILogger<NotificationSeeder> logger) : SeederBase(logger)
{
    private readonly Random _random = new();

    public override string Name => nameof(NotificationSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 160;

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Notification>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var notificationRepository = context.TenantUnitOfWork.Repository<Notification>();

        for (var i = 1; i <= 10; i++)
        {
            var notification = new Notification
            {
                Title = $"Test notification {i}",
                Message = $"Notification {i} description",
                CreatedDate = DateTime.SpecifyKind(
                    _random.UtcDate(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddDays(-1)),
                    DateTimeKind.Utc)
            };

            await notificationRepository.AddAsync(notification, cancellationToken);
            Logger.LogInformation("Created notification {Title}", notification.Title);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(10);
    }
}
