import { OpenIdConfiguration } from "angular-auth-oidc-client";
import { environment } from "@/env";

export const authConfig: OpenIdConfiguration = {
  authority: environment.idBaseUrl,
  postLoginRoute: "/",
  // forbiddenRoute: '/forbidden',
  unauthorizedRoute: "/unauthorized",
  redirectUrl: window.location.origin,
  postLogoutRedirectUri: window.location.origin,
  clientId: "logistics.officeapp",
  scope: "openid profile offline_access roles tenant logistics.api.tenant",
  responseType: "code",
  silentRenew: true,
  useRefreshToken: true,
  renewTimeBeforeTokenExpiresInSeconds: 30,
};
