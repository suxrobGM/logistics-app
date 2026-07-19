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
  getAccessToken,
  PERMISSION_CHECKER,
  provideSpartanHlm,
  UPGRADE_HANDLER,
} from "@logistics/shared";
import { provideApi, TENANT_ID_PROVIDER, tenantInterceptor } from "@logistics/shared/api";
import { AUTH_HOOKS, provideAppAuth, type AuthHooks } from "@logistics/shared/auth";
import {
  FEATURE_PROVIDER,
  I18nService,
  TENANT_SETTINGS_PROVIDER,
} from "@logistics/shared/services";
import { provideTranslateService } from "@ngx-translate/core";
import { provideTranslateHttpLoader } from "@ngx-translate/http-loader";
import { provideMapboxGL } from "ngx-mapbox-gl";
import { authOidcOptions, PermissionService } from "@/core/auth";
import { TmsFeatureProvider } from "@/core/services/feature.provider";
import { TmsTenantSettingsProvider } from "@/core/services/tenant-settings.provider";
import { TenantService } from "@/core/services/tenant.service";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";
import { environment } from "@/env";
import { appRoutes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideSpartanHlm(),
    provideAppAuth({ oidc: authOidcOptions }),
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
    provideMapboxGL({ accessToken: environment.mapboxToken }),
    provideTranslateService({ fallbackLang: "en", lang: "en" }),
    provideTranslateHttpLoader({ prefix: "/assets/i18n/", suffix: ".json" }),
    provideAppInitializer(() => {
      inject(I18nService).init({ supportedLanguages: ["en"] });
    }),

    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
    {
      provide: AUTH_HOOKS,
      useFactory: (): AuthHooks => {
        const tenantService = inject(TenantService);
        return { onAuthenticated: (user) => tenantService.setTenantId(user.tenant!) };
      },
    },
    { provide: TENANT_ID_PROVIDER, useExisting: TenantService },
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: TmsTenantSettingsProvider },
    { provide: FEATURE_PROVIDER, useExisting: TmsFeatureProvider },
    { provide: UPGRADE_HANDLER, useExisting: UpgradePromptService },
  ],
};
