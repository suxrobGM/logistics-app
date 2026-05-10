import {
  importProvidersFrom,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  type ApplicationConfig,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import { getAccessToken, provideApi } from "@logistics/shared";
import { I18nService, TENANT_SETTINGS_PROVIDER } from "@logistics/shared/services";
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
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAuth({ config: authConfig }),
    provideRouter(routes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: ".dark-mode",
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
