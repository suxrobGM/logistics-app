import type { OpenIdConfiguration } from "angular-auth-oidc-client";
import { environment } from "@/env";

export const authConfig: OpenIdConfiguration = {
  authority: environment.identityServerUrl,
  postLoginRoute: "/",
  // forbiddenRoute: '/forbidden',
  unauthorizedRoute: "/unauthorized",
  redirectUrl: window.location.origin,
  postLogoutRedirectUri: window.location.origin,
  clientId: "logisticsx.tmsportal",
  scope: "openid profile offline_access roles tenant logisticsx.api.tenant",
  responseType: "code",
  silentRenew: true,
  useRefreshToken: true,
  renewTimeBeforeTokenExpiresInSeconds: 30,
};
