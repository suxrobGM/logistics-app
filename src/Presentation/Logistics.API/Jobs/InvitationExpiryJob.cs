using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class InvitationExpiryJob
{
    public static void ScheduleJobs()
    {
        // Run daily at 3:00 AM UTC. Bulk-flips pending invitations whose ExpiresAt is in the past.
        RecurringJob.AddOrUpdate<IInvitationExpiryService>(
            "ExpireStaleInvitations",
            x => x.ExpireStaleAsync(default),
            Cron.Daily(3));
    }
}
