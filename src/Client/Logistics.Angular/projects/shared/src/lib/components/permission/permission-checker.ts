import { InjectionToken } from "@angular/core";

/**
 * Interface for permission checking services.
 * Applications should implement this interface and provide it via the PERMISSION_CHECKER token.
 */
export interface PermissionChecker {
  /**
   * Check if user has a specific permission.
   */
  hasPermission(permission: string): boolean;

  /**
   * Check if user has at least one of the specified permissions.
   */
  hasAnyPermission(...permissions: string[]): boolean;

  /**
   * Check if user has all of the specified permissions.
   */
  hasAllPermissions(...permissions: string[]): boolean;
}

/**
 * Injection token for the permission checker service.
 *
 * Applications should provide this token with their permission service:
 * ```typescript
 * providers: [
 *   { provide: PERMISSION_CHECKER, useExisting: PermissionService }
 * ]
 * ```
 */
export const PERMISSION_CHECKER = new InjectionToken<PermissionChecker>("PERMISSION_CHECKER");
