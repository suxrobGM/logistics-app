import type { OpenIdConfiguration } from "angular-auth-oidc-client";

/**
 * The per-portal knobs that differ between the OIDC clients. Everything else in the
 * OpenID configuration is identical across the portals and lives in {@link createOidcConfig}.
 */
export interface AppOidcOptions {
  authority: string;
  clientId: string;
  scope: string;
  postLoginRoute: string;
  unauthorizedRoute: string;
}

/**
 * Build the shared `angular-auth-oidc-client` configuration from the five per-portal options.
 *
 * `window.location.origin` is read INSIDE the function on purpose: this file is part of the
 * shared library which is also imported by the SSR website, where `window` does not exist at
 * module-evaluation time.
 */
export function createOidcConfig(o: AppOidcOptions): OpenIdConfiguration {
  const origin = window.location.origin;

  return {
    authority: o.authority,
    postLoginRoute: o.postLoginRoute,
    unauthorizedRoute: o.unauthorizedRoute,
    redirectUrl: origin,
    postLogoutRedirectUri: origin,
    clientId: o.clientId,
    scope: o.scope,
    responseType: "code",
    silentRenew: true,
    useRefreshToken: true,
    renewTimeBeforeTokenExpiresInSeconds: 30,
  };
}
