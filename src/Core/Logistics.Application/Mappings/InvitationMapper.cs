using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class InvitationMapper
{
    [MapperIgnoreSource(nameof(Invitation.Tenant))]
    [MapperIgnoreSource(nameof(Invitation.AcceptedByUser))]
    [MapperIgnoreSource(nameof(Invitation.InvitedByUser))]
    [MapperIgnoreTarget(nameof(InvitationDto.TenantName))]
    [MapperIgnoreTarget(nameof(InvitationDto.InvitedByName))]
    [MapperIgnoreTarget(nameof(InvitationDto.CustomerName))]
    [MapPropertyFromSource(nameof(InvitationDto.TenantRoleDisplayName), Use = nameof(GetInvitedRoleDisplayName))]
    private static partial InvitationDto MapInvitation(Invitation entity);

    extension(Invitation entity)
    {
        /// <summary>CustomerName stays null because customers live in the tenant database.</summary>
        public InvitationDto ToDto()
        {
            var dto = MapInvitation(entity);
            return dto with
            {
                TenantName = entity.Tenant?.DisplayName ?? string.Empty,
                InvitedByName = entity.InvitedByUser?.GetFullName() ?? string.Empty
            };
        }

        public InvitationDto ToDto(string tenantName, string invitedByName, string? customerName = null)
        {
            var dto = entity.ToDto();
            return dto with
            {
                TenantName = tenantName,
                InvitedByName = invitedByName,
                CustomerName = customerName
            };
        }
    }

    private static string GetInvitedRoleDisplayName(Invitation entity) =>
        entity.Type == InvitationType.AppUser
            ? GetAppRoleDisplayName(entity.AppRole)
            : GetRoleDisplayName(entity.TenantRole);

    public static string GetRoleDisplayName(string? role) => role switch
    {
        TenantRoles.Owner => "Owner",
        TenantRoles.Manager => "Manager",
        TenantRoles.Dispatcher => "Dispatcher",
        TenantRoles.Driver => "Driver",
        TenantRoles.Customer => "Customer",
        _ => role ?? string.Empty
    };

    /// <summary>
    /// Gets the display name for an app-level role, sourced from <see cref="AppRoles.GetValues"/>.
    /// </summary>
    public static string GetAppRoleDisplayName(string? role) =>
        AppRoles.GetValues().FirstOrDefault(v => v.Value == role)?.DisplayName ?? role ?? string.Empty;
}
