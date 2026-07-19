import { InjectionToken } from "@angular/core";

/**
 * Internal knobs for the shared AuthService. Not part of the public auth barrel - portals
 * configure these through {@link provideAppAuth}, never by injecting the token directly.
 */
export interface AuthOptions {
  /** Whether `checkAuth` should load the user's permissions during initialization. */
  loadPermissionsOnInit: boolean;
}

export const AUTH_OPTIONS = new InjectionToken<AuthOptions>("AUTH_OPTIONS");
