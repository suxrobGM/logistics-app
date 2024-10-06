import {OpenIdConfiguration} from "angular-auth-oidc-client";
import {GLOBAL_CONFIG} from "./global.config";

export const AUTH_CONFIG: OpenIdConfiguration = {
  authority: GLOBAL_CONFIG.idHost,
  postLoginRoute: "/",
  // forbiddenRoute: '/forbidden',
  unauthorizedRoute: "/unauthorized",
  redirectUrl: window.location.origin,
  postLogoutRedirectUri: window.location.origin,
  clientId: "logistics.officeapp",
  scope: "openid profile offline_access roles logistics.api.tenant",
  responseType: "code",
  silentRenew: true,
  useRefreshToken: true,
  renewTimeBeforeTokenExpiresInSeconds: 30,
};
