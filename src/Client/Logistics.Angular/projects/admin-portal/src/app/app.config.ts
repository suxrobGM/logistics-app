import {
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  type ApplicationConfig,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding, withRouterConfig } from "@angular/router";
import {
  BASE_NG_ICONS,
  getAccessToken,
  PERMISSION_CHECKER,
  provideSpartanHlm,
} from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { provideIcons } from "@ng-icons/core";
import { provideAuth } from "angular-auth-oidc-client";
import { authConfig, PermissionService } from "@/core/auth";
import { environment } from "@/env";
import { ADMIN_NG_ICONS } from "@/shared/icons/lucide-icons";
import { appRoutes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideSpartanHlm(),
    provideAuth({ config: authConfig }),
    provideRouter(
      appRoutes,
      withComponentInputBinding(),
      // Angular 22 changed the default paramsInheritanceStrategy from 'emptyOnly' to 'always'.
      // Pin 'emptyOnly' so children don't silently inherit parent params/data (no migration exists).
      withRouterConfig({ paramsInheritanceStrategy: "emptyOnly" }),
    ),
    importProvidersFrom(BrowserModule),
    provideApi({
      baseUrl: environment.apiUrl,
      tokenGetter: () => getAccessToken("adminportal"),
    }),
    provideIcons({ ...BASE_NG_ICONS, ...ADMIN_NG_ICONS }),

    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
  ],
};
