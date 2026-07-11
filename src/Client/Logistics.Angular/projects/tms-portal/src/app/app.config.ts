import {
  importProvidersFrom,
  inject,
  provideAppInitializer,
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
  UPGRADE_HANDLER,
} from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import {
  FEATURE_PROVIDER,
  I18nService,
  TENANT_SETTINGS_PROVIDER,
} from "@logistics/shared/services";
import { provideIcons } from "@ng-icons/core";
import { provideTranslateService } from "@ngx-translate/core";
import { provideTranslateHttpLoader } from "@ngx-translate/http-loader";
import { provideAuth } from "angular-auth-oidc-client";
import { provideMapboxGL } from "ngx-mapbox-gl";
import { providePrimeNG } from "primeng/config";
import { authConfig, PermissionService } from "@/core/auth";
import { tenantInterceptor } from "@/core/interceptors";
import { TmsFeatureProvider } from "@/core/services/feature.provider";
import { TmsTenantSettingsProvider } from "@/core/services/tenant-settings.provider";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";
import { TmsPreset, TmsThemeOptions } from "@/core/theme/primeng-preset";
import { environment } from "@/env";
import { TMS_NG_ICONS } from "@/shared/icons/lucide-icons";
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
    provideIcons({ ...BASE_NG_ICONS, ...TMS_NG_ICONS }),
    provideTranslateService({ fallbackLang: "en", lang: "en" }),
    provideTranslateHttpLoader({ prefix: "/assets/i18n/", suffix: ".json" }),
    provideAppInitializer(() => {
      inject(I18nService).init({ supportedLanguages: ["en"] });
    }),

    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: TmsTenantSettingsProvider },
    { provide: FEATURE_PROVIDER, useExisting: TmsFeatureProvider },
    { provide: UPGRADE_HANDLER, useExisting: UpgradePromptService },
  ],
};
