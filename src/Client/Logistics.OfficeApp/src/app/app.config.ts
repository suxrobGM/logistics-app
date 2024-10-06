import {provideHttpClient, withInterceptors} from "@angular/common/http";
import {ApplicationConfig, importProvidersFrom} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {provideAnimations} from "@angular/platform-browser/animations";
import {provideRouter} from "@angular/router";
import {provideAuth} from "angular-auth-oidc-client";
import {APP_ROUTES} from "./app.routes";
import {AUTH_CONFIG} from "./configs";
import {tenantInterceptor, tokenInterceptor} from "./core/interceptors";
import { MessageService } from "primeng/api";

export const appConfig: ApplicationConfig = {
  providers: [
    provideAuth({config: AUTH_CONFIG}),
    provideRouter(APP_ROUTES),
    importProvidersFrom(BrowserModule),
    provideAnimations(),
    provideHttpClient(withInterceptors([tenantInterceptor, tokenInterceptor])),
    MessageService,
  ],
};
