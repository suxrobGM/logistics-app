using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Queries;

internal sealed class GetAdminsHandler(UserManager<User> userManager)
    : IAppRequestHandler<GetAdminsQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetAdminsQuery req, CancellationToken ct)
    {
        var superAdmins = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
        var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);

        var superAdminIds = superAdmins.Select(u => u.Id).ToHashSet();

        // SuperAdmin display takes precedence when a user happens to hold both roles.
        var combined = superAdmins
            .Select(u => (User: u, Role: AppRoles.SuperAdmin))
            .Concat(admins
                .Where(u => !superAdminIds.Contains(u.Id))
                .Select(u => (User: u, Role: AppRoles.Admin)))
            .ToList();

        var search = req.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            combined = combined
                .Where(x =>
                    (x.User.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.User.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.User.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        var totalItems = combined.Count;

        var items = combined
            .OrderBy(x => x.User.Email)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(x => x.User.ToDto(InvitationMapper.GetAppRoleDisplayName(x.Role)))
            .ToArray();

        return PagedResult<UserDto>.Ok(items, totalItems, req.PageSize);
    }
}
