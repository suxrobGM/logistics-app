import type { OpenIdConfiguration } from "angular-auth-oidc-client";
import { environment } from "@/env";

export const authConfig: OpenIdConfiguration = {
  authority: environment.identityServerUrl,
  postLoginRoute: "/",
  unauthorizedRoute: "/login",
  redirectUrl: window.location.origin,
  postLogoutRedirectUri: window.location.origin,
  clientId: "logisticsx.customerportal",
  scope: "openid profile offline_access roles tenant logisticsx.api.tenant",
  responseType: "code",
  silentRenew: true,
  useRefreshToken: true,
  renewTimeBeforeTokenExpiresInSeconds: 30,
};
