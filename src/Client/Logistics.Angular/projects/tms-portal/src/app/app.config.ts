import type { ApplicationConfig } from "@angular/core";
import { importProvidersFrom, provideBrowserGlobalErrorListeners } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import { getAccessToken, PERMISSION_CHECKER } from "@logistics/shared";
import { FEATURE_PROVIDER, TENANT_SETTINGS_PROVIDER } from "@logistics/shared/services";
import { provideApi } from "@logistics/shared/api";
import { PermissionService } from "@/core/auth";
import { provideAuth } from "angular-auth-oidc-client";
import { provideMapboxGL } from "ngx-mapbox-gl";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { authConfig } from "@/core/auth";
import { tenantInterceptor } from "@/core/interceptors";
import { TmsTenantSettingsProvider } from "@/core/services/tenant-settings.provider";
import { TmsFeatureProvider } from "@/core/services/feature.provider";
import { environment } from "@/env";
import { appRoutes } from "./app.routes";
import { TmsPreset, TmsThemeOptions } from "@/core/theme/primeng-preset";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAuth({ config: authConfig }),
    provideRouter(appRoutes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    provideApi({
      baseUrl: environment.apiUrl,
      interceptors: [tenantInterceptor],
      tokenGetter: () => getAccessToken("tmsportal"),
    }),
    providePrimeNG({
      theme: {
        preset: TmsPreset,
        options: TmsThemeOptions,
      },
    }),
    provideMapboxGL({ accessToken: environment.mapboxToken }),

    MessageService,
    ConfirmationService,
    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: TmsTenantSettingsProvider },
    { provide: FEATURE_PROVIDER, useExisting: TmsFeatureProvider },
  ],
};
