import type { OpenIdConfiguration } from "angular-auth-oidc-client";
import { environment } from "@/env";

export const authConfig: OpenIdConfiguration = {
  authority: environment.identityServerUrl,
  postLoginRoute: "/home",
  unauthorizedRoute: "/unauthorized",
  redirectUrl: window.location.origin,
  postLogoutRedirectUri: window.location.origin,
  clientId: "logistics.adminportal",
  scope: "openid profile offline_access roles tenant logistics.api.admin",
  responseType: "code",
  silentRenew: true,
  useRefreshToken: true,
  renewTimeBeforeTokenExpiresInSeconds: 30,
};
