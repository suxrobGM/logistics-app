import type { AppOidcOptions } from "@logistics/shared/auth";
import { environment } from "@/env";

export const authOidcOptions: AppOidcOptions = {
  authority: environment.identityServerUrl,
  clientId: "logisticsx.tmsportal",
  scope: "openid profile offline_access roles tenant logisticsx.api.tenant",
  postLoginRoute: "/",
  unauthorizedRoute: "/unauthorized",
};
