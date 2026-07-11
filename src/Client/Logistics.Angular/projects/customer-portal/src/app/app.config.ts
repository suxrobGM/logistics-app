import {
  importProvidersFrom,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  type ApplicationConfig,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding, withRouterConfig } from "@angular/router";
import { BASE_NG_ICONS, getAccessToken, provideApi, provideSpartanHlm } from "@logistics/shared";
import { I18nService, TENANT_SETTINGS_PROVIDER } from "@logistics/shared/services";
import { provideIcons } from "@ng-icons/core";
import { provideTranslateService } from "@ngx-translate/core";
import { provideTranslateHttpLoader } from "@ngx-translate/http-loader";
import Aura from "@primeuix/themes/aura";
import { provideAuth } from "angular-auth-oidc-client";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { authConfig } from "@/core/auth";
import { tenantInterceptor } from "@/core/interceptors";
import { CustomerPortalSettingsProvider } from "@/core/services";
import { environment } from "@/env";
import { CUSTOMER_NG_ICONS } from "@/shared/icons/lucide-icons";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideIcons({ ...BASE_NG_ICONS, ...CUSTOMER_NG_ICONS }),
    provideBrowserGlobalErrorListeners(),
    provideSpartanHlm(),
    provideAuth({ config: authConfig }),
    provideRouter(
      routes,
      withComponentInputBinding(),
      // Angular 22 changed the default paramsInheritanceStrategy from 'emptyOnly' to 'always'.
      // Pin 'emptyOnly' so children don't silently inherit parent params/data (no migration exists).
      withRouterConfig({ paramsInheritanceStrategy: "emptyOnly" }),
    ),
    importProvidersFrom(BrowserModule),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: ".dark-theme",
        },
      },
    }),
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

    MessageService,
    ConfirmationService,

    // Localization provider
    { provide: TENANT_SETTINGS_PROVIDER, useExisting: CustomerPortalSettingsProvider },
  ],
};
