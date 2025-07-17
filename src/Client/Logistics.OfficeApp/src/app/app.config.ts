import {provideHttpClient, withInterceptors} from "@angular/common/http";
import {
  ApplicationConfig,
  importProvidersFrom,
  provideZonelessChangeDetection,
} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {provideAnimationsAsync} from "@angular/platform-browser/animations/async";
import {provideRouter, withComponentInputBinding} from "@angular/router";
import Aura from "@primeuix/themes/aura";
import {provideAuth} from "angular-auth-oidc-client";
import {provideMapboxGL} from "ngx-mapbox-gl";
import {ConfirmationService, MessageService} from "primeng/api";
import {providePrimeNG} from "primeng/config";
import {authConfig} from "@/core/auth";
import {tenantInterceptor, tokenInterceptor} from "@/core/interceptors";
import {environment} from "@/env";
import {appRoutes} from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideAuth({config: authConfig}),
    provideRouter(appRoutes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([tenantInterceptor, tokenInterceptor])),
    providePrimeNG({
      theme: {
        preset: Aura,
      },
    }),
    provideMapboxGL({
      accessToken: environment.mapboxToken,
    }),

    MessageService,
    ConfirmationService,
  ],
};
