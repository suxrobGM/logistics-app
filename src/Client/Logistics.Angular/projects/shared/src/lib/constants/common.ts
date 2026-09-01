import type { TenantSettings } from "../api";

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

/**
 * Roles assignable to a truck or named on a driver-facing record. Owner is here because an
 * owner-operator drives their own truck - the same reason Owner carries `Permission.Driver.*`.
 */
export const DRIVING_ROLES: readonly TenantRoleValue[] = [TenantRole.Driver, TenantRole.Owner];

/**
 * Default tenant localization settings matches with US locales
 */
export const DEFAULT_TENANT_SETTINGS: TenantSettings = {
  distanceUnit: "miles",
  currency: "usd",
  dateFormat: "us",
  timezone: "America/New_York",
  weightUnit: "pounds",
  operatingMode: "fleet",
};

/** Licensing enquiries, linked from the noncommercial banner and the website license page. */
export const LICENSE_CONTACT_HREF =
  "mailto:suxrobgm@gmail.com?subject=LogisticsX%20commercial%20license";
