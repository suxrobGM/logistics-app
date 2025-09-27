import {
  ApplicationConfig,
  importProvidersFrom,
  provideZonelessChangeDetection,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import Aura from "@primeuix/themes/aura";
import { provideAuth } from "angular-auth-oidc-client";
import { provideMapboxGL } from "ngx-mapbox-gl";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { provideApi } from "@/core/api";
import { authConfig } from "@/core/auth";
import { tenantInterceptor } from "@/core/interceptors";
import { environment } from "@/env";
import { getAccessToken } from "@/shared/utils";
import { appRoutes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideAuth({ config: authConfig }),
    provideRouter(appRoutes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    provideAnimationsAsync(),
    provideApi({
      baseUrl: environment.apiBaseUrl,
      interceptors: [tenantInterceptor],
      tokenGetter: getAccessToken,
    }),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false, // force light only
        },
      },
    }),
    provideMapboxGL({ accessToken: environment.mapboxToken }),

    MessageService,
    ConfirmationService,
  ],
};
