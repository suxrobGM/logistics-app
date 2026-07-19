import { makeEnvironmentProviders, type EnvironmentProviders } from "@angular/core";
import { provideAuth } from "angular-auth-oidc-client";
import { AUTH_OPTIONS } from "./auth-options";
import { createOidcConfig, type AppOidcOptions } from "./oidc-config";

/**
 * Portal-level auth configuration passed to {@link provideAppAuth}.
 */
export interface AppAuthConfig {
  oidc: AppOidcOptions;
  /** Whether to load permissions during the initial auth check. Defaults to `true`. */
  loadPermissionsOnInit?: boolean;
}

/**
 * Wire up OIDC and the shared AuthService options for a portal. Replaces the raw
 * `provideAuth({ config })` call in each portal's `app.config.ts`.
 */
export function provideAppAuth(config: AppAuthConfig): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideAuth({ config: createOidcConfig(config.oidc) }),
    {
      provide: AUTH_OPTIONS,
      useValue: { loadPermissionsOnInit: config.loadPermissionsOnInit ?? true },
    },
  ]);
}
