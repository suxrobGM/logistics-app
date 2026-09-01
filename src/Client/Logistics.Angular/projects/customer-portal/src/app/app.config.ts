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
  provideApi,
  provideSpartanHlm,
  TENANT_ID_PROVIDER,
  tenantInterceptor,
} from "@logistics/shared";
import { provideAppAuth } from "@logistics/shared/auth";
import { I18nService, TENANT_SETTINGS_PROVIDER } from "@logistics/shared/services";
import { provideTranslateService } from "@ngx-translate/core";
import { provideTranslateHttpLoader } from "@ngx-translate/http-loader";
import { authOidcOptions } from "@/core/auth";
import { CustomerPortalSettingsProvider, TenantContextService } from "@/core/services";
import { environment } from "@/env";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideSpartanHlm(),
    provideAppAuth({ oidc: authOidcOptions, loadPermissionsOnInit: false }),
    provideRouter(
      routes,
      withComponentInputBinding(),
      // Angular 22 changed the default paramsInheritanceStrategy from 'emptyOnly' to 'always'.
      // Pin 'emptyOnly' so children don't silently inherit parent params/data (no migration exists).
      withRouterConfig({ paramsInheritanceStrategy: "emptyOnly" }),
    ),
    importProvidersFrom(BrowserModule),
    provideApi({
      baseUrl: environment.apiUrl,
      tokenGetter: () => getAccessToken("customerportal"),
      interceptors: [tenantInterceptor],
    }),
    provideTranslateService({ fallbackLang: "en", lang: "en" }),
    provideTranslateHttpLoader({ prefix: "/assets/i18n/", suffix: ".json" }),
    provideAppInitializer(() => {
      inject(I18nService).init({ supportedLanguages: ["en"] });
    }),

    { provide: TENANT_ID_PROVIDER, useExisting: TenantContextService },

    // Localization provider
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: CustomerPortalSettingsProvider },
  ],
};
