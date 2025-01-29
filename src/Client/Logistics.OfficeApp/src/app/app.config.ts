import {provideHttpClient, withInterceptors} from "@angular/common/http";
import {ApplicationConfig, importProvidersFrom} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {provideAnimations} from "@angular/platform-browser/animations";
import {provideRouter, withComponentInputBinding} from "@angular/router";
import {provideAuth} from "angular-auth-oidc-client";
import {MessageService} from "primeng/api";
import {providePrimeNG} from "primeng/config";
import Aura from "@primeng/themes/aura";
import {tenantInterceptor, tokenInterceptor} from "@/core/interceptors";
import {AUTH_CONFIG} from "@/configs";
import {appRoutes} from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideAuth({config: AUTH_CONFIG}),
    provideRouter(appRoutes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    provideAnimations(),
    provideHttpClient(withInterceptors([tenantInterceptor, tokenInterceptor])),
    providePrimeNG({
      theme: {
        preset: Aura,
      },
    }),
    MessageService,
  ],
};
