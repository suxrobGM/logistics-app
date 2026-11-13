namespace Logistics.Domain.Primitives.Enums;

public enum InvitationType
{
    Employee,
    CustomerUser,

    /// <summary>
    /// An app-level user (e.g. an Admin) that is not scoped to any tenant.
    /// </summary>
    AppUser
}
