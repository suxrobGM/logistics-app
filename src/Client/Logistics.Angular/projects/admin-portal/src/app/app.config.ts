import type { ApplicationConfig } from "@angular/core";
import { importProvidersFrom, provideBrowserGlobalErrorListeners } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import { PERMISSION_CHECKER, getAccessToken } from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import Aura from "@primeuix/themes/aura";
import { provideAuth } from "angular-auth-oidc-client";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { PermissionService } from "@/core/auth";
import { authConfig } from "@/core/auth";
import { environment } from "@/env";
import { appRoutes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAuth({ config: authConfig }),
    provideRouter(appRoutes, withComponentInputBinding()),
    importProvidersFrom(BrowserModule),
    provideApi({
      baseUrl: environment.apiUrl,
      tokenGetter: () => getAccessToken("adminportal"),
    }),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false, // force light only
        },
      },
    }),

    MessageService,
    ConfirmationService,
    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
  ],
};
