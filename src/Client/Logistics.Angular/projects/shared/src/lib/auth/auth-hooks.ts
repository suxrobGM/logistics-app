import { InjectionToken } from "@angular/core";
import type { UserData } from "../models";

/**
 * Optional per-portal side effects that run at well-defined points in the auth lifecycle.
 * Provide an implementation via the {@link AUTH_HOOKS} token to hook into the shared AuthService
 * without subclassing it (e.g. the TMS portal seeds the tenant id on authentication).
 */
export interface AuthHooks {
  /**
   * Runs after `checkAuth` has set the user data and before permissions are loaded.
   * May be async - the service awaits it before completing initialization.
   */
  onAuthenticated?(user: UserData): void | Promise<void>;

  /**
   * Runs after the user has been logged out and local state has been cleared.
   */
  onLoggedOut?(): void;
}

export const AUTH_HOOKS = new InjectionToken<AuthHooks>("AUTH_HOOKS");
