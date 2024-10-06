import {OpenIdConfiguration} from "angular-auth-oidc-client";
import {AppConfig} from "./app.config";

export const AuthConfig: OpenIdConfiguration = {
  authority: AppConfig.idHost,
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
