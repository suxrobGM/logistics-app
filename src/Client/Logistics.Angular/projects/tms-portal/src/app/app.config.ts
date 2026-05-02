import {
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  type ApplicationConfig,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import {
  BASE_LUCIDE_ICONS,
  getAccessToken,
  PERMISSION_CHECKER,
  UPGRADE_HANDLER,
} from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { FEATURE_PROVIDER, TENANT_SETTINGS_PROVIDER } from "@logistics/shared/services";
import { provideLucideIcons } from "@lucide/angular";
import { provideAuth } from "angular-auth-oidc-client";
import { provideMapboxGL } from "ngx-mapbox-gl";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { authConfig, PermissionService } from "@/core/auth";
import { tenantInterceptor } from "@/core/interceptors";
import { TmsFeatureProvider } from "@/core/services/feature.provider";
import { TmsTenantSettingsProvider } from "@/core/services/tenant-settings.provider";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";
import { TmsPreset, TmsThemeOptions } from "@/core/theme/primeng-preset";
import { environment } from "@/env";
import { TMS_LUCIDE_ICONS } from "@/shared/icons/lucide-icons";
import { appRoutes } from "./app.routes";

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
    provideLucideIcons(...BASE_LUCIDE_ICONS, ...TMS_LUCIDE_ICONS),

    MessageService,
    ConfirmationService,
    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: TmsTenantSettingsProvider },
    { provide: FEATURE_PROVIDER, useExisting: TmsFeatureProvider },
    { provide: UPGRADE_HANDLER, useExisting: UpgradePromptService },
  ],
};
