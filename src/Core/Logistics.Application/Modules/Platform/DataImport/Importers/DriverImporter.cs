using Logistics.Application.Modules.IdentityAccess.Invitations.Commands;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Identity.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Drivers match on email. Each new driver gets a password-less master user (an employee's id is
///     its user's id) and a Driver-role employee, so they can be dispatched before anyone signs in.
///     An invitation, sent now or later, lets them set a password on that same user.
/// </summary>
internal sealed class DriverImporter(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    IUserService userService,
    IMediator mediator) : EntityImporter<DriverImporter.Draft>
{
    public override ImportEntityType EntityType => ImportEntityType.Drivers;

    protected override Draft Parse(ImportRow row, ImportOptions options)
    {
        var draft = new Draft(row, row.Get(Email)?.ToLowerInvariant() ?? "");
        if (draft.ReadRequired(Email, "Email") is { } email && !IsEmail(email))
        {
            draft.Errors.Add($"Email '{email}' is not a valid address.");
        }

        (draft.FirstName, draft.LastName) = (row.Get(FirstName), row.Get(LastName));
        if ((draft.FirstName is null || draft.LastName is null) && row.Get(FullName) is { } fullName)
        {
            var (first, last) = SplitName(fullName);
            draft.FirstName ??= first;
            draft.LastName ??= last;
        }

        if (draft.FirstName is null || draft.LastName is null)
        {
            draft.Errors.Add("First and last name are required (or a full name column).");
        }

        draft.HireDate = draft.ReadDate(HireDate, "Hire date", options);
        draft.Address = draft.ReadAddress(null, "Address", options, required: false);
        return draft;
    }

    protected override async Task ResolveAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var emails = drafts.Select(d => d.Key).ToArray();
        var employees = await tenantUow.Repository<Employee>().Query()
            .Where(e => emails.Contains(e.Email.ToLower()))
            .ToListAsync(ct);
        var employeeIds = employees.Select(e => e.Id).ToArray();
        var users = await masterUow.Repository<User>().Query()
            .Where(u => employeeIds.Contains(u.Id) || (u.Email != null && emails.Contains(u.Email.ToLower())))
            .ToListAsync(ct);
        var tenantId = tenantUow.GetCurrentTenant().Id;

        foreach (var draft in drafts)
        {
            draft.Existing = employees.FirstOrDefault(e => e.Email.Equals(draft.Key, StringComparison.OrdinalIgnoreCase));
            draft.Exists = draft.Existing is not null;
            draft.User = draft.Existing is { } employee
                ? users.FirstOrDefault(u => u.Id == employee.Id)
                : users.FirstOrDefault(u => u.Email!.Equals(draft.Key, StringComparison.OrdinalIgnoreCase));

            if (draft.Existing is null && draft.User is { TenantId: { } userTenant } && userTenant != tenantId)
            {
                draft.Errors.Add($"'{draft.Key}' already has an account with another company; invite them instead.");
            }
        }
    }

    protected override async Task WriteAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var driverRole = await tenantUow.Repository<TenantRole>().GetAsync(r => r.Name == TenantRoles.Driver, ct);
        var created = new List<Draft>();
        var renamedUsers = new List<User>();

        foreach (var draft in drafts)
        {
            var user = draft.User;
            if (user is null)
            {
                user = new User
                {
                    UserName = draft.Key,
                    Email = draft.Key,
                    FirstName = draft.FirstName!,
                    LastName = draft.LastName!,
                    PhoneNumber = draft.Row.Get(Phone),
                    TenantId = tenant.Id
                };

                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    draft.Errors.Add(string.Join(" ", result.Errors.Select(e => e.Description)));
                    continue;
                }
            }
            else
            {
                var renamed = ApplyProfile(user, draft);
                if (renamed || user.TenantId is null)
                {
                    user.TenantId ??= tenant.Id;
                    await userManager.UpdateAsync(user);
                }

                if (renamed)
                {
                    renamedUsers.Add(user);
                }
            }

            if (draft.Existing is { } employee)
            {
                employee.Address = draft.Address ?? employee.Address;
                employee.JoinedDate = draft.HireDate ?? employee.JoinedDate;
                continue;
            }

            var driver = Employee.CreateEmployeeFromUser(user, options.Currency);
            driver.Address = draft.Address;
            driver.JoinedDate = draft.HireDate ?? DateTime.UtcNow;
            driver.Role = driverRole;
            await tenantUow.Repository<Employee>().AddAsync(driver, ct);
            created.Add(draft);
        }

        await tenantUow.SaveChangesAsync(ct);

        foreach (var user in renamedUsers)
        {
            await userService.SyncTenantProfilesAsync(user, ct);
        }

        if (options.SendInvitations)
        {
            await InviteAsync(created, ct);
        }
    }

    /// <summary>The file's name and phone go onto the master user, which every tenant row copies.</summary>
    private static bool ApplyProfile(User user, Draft draft)
    {
        var firstName = draft.FirstName ?? user.FirstName;
        var lastName = draft.LastName ?? user.LastName;
        var phone = draft.Row.Get(Phone) ?? user.PhoneNumber;
        if (firstName == user.FirstName && lastName == user.LastName && phone == user.PhoneNumber)
        {
            return false;
        }

        (user.FirstName, user.LastName, user.PhoneNumber) = (firstName, lastName, phone);
        return true;
    }

    private async Task InviteAsync(IEnumerable<Draft> drafts, CancellationToken ct)
    {
        foreach (var draft in drafts)
        {
            var invited = await mediator.Send(new CreateInvitationCommand
            {
                Email = draft.Key,
                Type = InvitationType.Employee,
                TenantRole = TenantRoles.Driver
            }, ct);

            draft.Notices.Add(invited.IsSuccess
                ? "Invitation sent."
                : $"Driver imported, but the invitation failed: {invited.Error}");
        }
    }

    private static bool IsEmail(string value)
    {
        var at = value.IndexOf('@');
        return at > 0 && at == value.LastIndexOf('@') && value.IndexOf('.', at) > at + 1 && !value.Contains(' ');
    }

    /// <summary>"Doe, Jane" is last-first; otherwise the first word is the first name.</summary>
    private static (string? First, string? Last) SplitName(string fullName)
    {
        var comma = fullName.Split(',', 2, StringSplitOptions.TrimEntries);
        if (comma.Length == 2)
        {
            return (NullIfEmpty(comma[1]), NullIfEmpty(comma[0]));
        }

        var words = fullName.Split(' ', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return words.Length == 2 ? (words[0], words[1]) : (NullIfEmpty(fullName.Trim()), null);
    }

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;

    internal sealed class Draft(ImportRow row, string key) : ImportDraft(row, key)
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? HireDate { get; set; }
        public Address? Address { get; set; }
        public Employee? Existing { get; set; }
        public User? User { get; set; }
    }
}
