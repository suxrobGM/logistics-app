/**
 * Represents an empty/null GUID value.
 * Used to check if a GUID is effectively null/unassigned.
 */
export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

/**
 * Checks if a GUID string is empty (null, undefined, empty string, or empty GUID).
 */
export function isEmptyGuid(guid: string | null | undefined): boolean {
  return !guid || guid === EMPTY_GUID;
}

export const TenantRole = {
  Owner: "tenant.owner",
  Manager: "tenant.manager",
  Dispatcher: "tenant.dispatcher",
  Driver: "tenant.driver",
} as const;

export type TenantRoleValue = (typeof TenantRole)[keyof typeof TenantRole];
