import {provideHttpClient, withInterceptors} from "@angular/common/http";
import {ApplicationConfig, importProvidersFrom} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {provideAnimationsAsync} from "@angular/platform-browser/animations/async";
import {provideRouter, withComponentInputBinding} from "@angular/router";
import Aura from "@primeuix/themes/aura";
import {provideAuth} from "angular-auth-oidc-client";
import {ConfirmationService, MessageService} from "primeng/api";
import {providePrimeNG} from "primeng/config";
import {authConfig} from "@/core/auth";
import {tenantInterceptor, tokenInterceptor} from "@/core/interceptors";
import {appRoutes} from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
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
    MessageService,
    ConfirmationService,
  ],
};
