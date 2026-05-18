using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Logistics.Application.Abstractions.Privacy;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Documents.Privacy;

/// <summary>
/// Builds a single ZIP archive containing every category of personal data we hold
/// for a given user. Cross-tenant: walks UserTenantAccess and switches the tenant
/// UoW to each tenant the user belongs to.
/// </summary>
internal sealed class DataExportService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<DataExportService> logger) : IDataExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Stream> GenerateExportAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await masterUow.Repository<User>().GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException($"User '{userId}' not found.");

        // Buffer the ZIP in a temp file so we don't OOM on large exports.
        var tempPath = Path.Combine(Path.GetTempPath(), $"export-{userId:N}-{Guid.NewGuid():N}.zip");
        var output = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None,
            bufferSize: 81920, FileOptions.DeleteOnClose | FileOptions.Asynchronous);

        try
        {
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                await WriteJsonAsync(archive, "profile.json", BuildProfile(user), ct);
                await WriteMasterDbDataAsync(archive, userId, ct);
                await WriteTenantsDataAsync(archive, user, ct);
                await WriteReadmeAsync(archive, user, ct);
            }

            output.Position = 0;
            return output;
        }
        catch
        {
            await output.DisposeAsync();
            throw;
        }
    }

    private static object BuildProfile(User user) => new
    {
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.PhoneNumber,
        user.UserName,
        user.CreatedAt,
        user.DeletionRequestedAt,
        user.AnonymizedAt
    };

    private async Task WriteMasterDbDataAsync(ZipArchive archive, Guid userId, CancellationToken ct)
    {
        var accesses = await masterUow.Repository<UserTenantAccess>()
            .Query()
            .Where(a => a.UserId == userId)
            .ToListAsync(ct);

        await WriteJsonAsync(archive, "tenant-access.json",
            accesses.Select(a => new { a.TenantId, a.CustomerUserId, a.CustomerName, a.IsActive, a.LastAccessedAt, a.CreatedAt }),
            ct);

        var consents = await masterUow.Repository<ConsentRecord>()
            .Query()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Timestamp)
            .ToListAsync(ct);

        await WriteJsonAsync(archive, "consents.json",
            consents.Select(c => new { c.ConsentType, c.Granted, c.Timestamp }),
            ct);

        var exports = await masterUow.Repository<DataExportRequest>()
            .Query()
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(ct);

        var deletions = await masterUow.Repository<DataDeletionRequest>()
            .Query()
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(ct);

        await WriteJsonAsync(archive, "data-requests.json", new
        {
            Exports = exports.Select(r => new { r.Id, r.RequestedAt, r.Status, r.ExpiresAt }),
            Deletions = deletions.Select(r => new { r.Id, r.RequestedAt, r.ScheduledFor, r.Status, r.Reason, r.CancelledAt, r.ProcessedAt })
        }, ct);
    }

    private async Task WriteTenantsDataAsync(ZipArchive archive, User user, CancellationToken ct)
    {
        var tenants = await masterUow.Repository<Tenant>()
            .Query()
            .Where(t => t.Users.Any(u => u.Id == user.Id))
            .ToListAsync(ct);

        foreach (var tenant in tenants)
        {
            try
            {
                tenantUow.SetCurrentTenant(tenant);
                var folder = $"tenants/{Slug(tenant.Name)}";

                var employee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);
                if (employee is not null)
                {
                    await WriteJsonAsync(archive, $"{folder}/employee.json", new
                    {
                        employee.Id,
                        employee.FirstName,
                        employee.LastName,
                        employee.Email,
                        employee.PhoneNumber,
                        employee.JoinedDate,
                        employee.Status,
                        employee.SalaryType
                    }, ct);
                }

                var customerUser = await tenantUow.Repository<CustomerUser>()
                    .Query()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id, ct);

                if (customerUser is not null)
                {
                    await WriteJsonAsync(archive, $"{folder}/customer-user.json", new
                    {
                        customerUser.Id,
                        customerUser.CustomerId,
                        customerUser.Email,
                        customerUser.DisplayName,
                        customerUser.IsActive,
                        customerUser.LastLoginAt,
                        customerUser.CreatedAt
                    }, ct);
                }

                var notifications = await tenantUow.Repository<Notification>()
                    .Query()
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(1000)
                    .ToListAsync(ct);

                if (notifications.Count > 0)
                {
                    await WriteJsonAsync(archive, $"{folder}/notifications.json",
                        notifications.Select(n => new { n.Title, n.Message, n.IsRead, n.CreatedDate }),
                        ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to export tenant '{TenantName}' for user '{UserId}'", tenant.Name, user.Id);
                await WriteTextAsync(archive, $"tenants/{Slug(tenant.Name)}/EXPORT_ERROR.txt",
                    $"Some data from this tenant could not be exported: {ex.Message}", ct);
            }
        }
    }

    private static async Task WriteReadmeAsync(ZipArchive archive, User user, CancellationToken ct)
    {
        var readme = $"""
            Personal data export for {user.GetFullName()} ({user.Email})
            Generated: {DateTime.UtcNow:O}

            Contents:
              profile.json          — your account profile (master)
              tenant-access.json    — organizations you have access to
              consents.json         — cookie / processing consents
              data-requests.json    — your past data export & deletion requests
              tenants/<name>/       — per-organization data
                employee.json       — employee record (if applicable)
                customer-user.json  — customer portal user (if applicable)
                notifications.json  — recent notifications visible to you

            Operational records (loads, trips, invoices, payments, audit logs)
            are retained for legal/financial compliance and are not included here.
            """;
        await WriteTextAsync(archive, "README.txt", readme, ct);
    }

    private static async Task WriteJsonAsync<T>(ZipArchive archive, string entryName, T payload, CancellationToken ct)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await JsonSerializer.SerializeAsync(stream, payload, JsonOptions, ct);
    }

    private static async Task WriteTextAsync(ZipArchive archive, string entryName, string text, CancellationToken ct)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        var bytes = Encoding.UTF8.GetBytes(text);
        await stream.WriteAsync(bytes, ct);
    }

    private static string Slug(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            sb.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        return sb.ToString().Trim('-');
    }
}
