import {
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  type ApplicationConfig,
} from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import { BASE_LUCIDE_ICONS, getAccessToken, PERMISSION_CHECKER } from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { provideLucideIcons } from "@lucide/angular";
import Aura from "@primeuix/themes/aura";
import { provideAuth } from "angular-auth-oidc-client";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { authConfig, PermissionService } from "@/core/auth";
import { environment } from "@/env";
import { ADMIN_LUCIDE_ICONS } from "@/shared/icons/lucide-icons";
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
    provideLucideIcons(...BASE_LUCIDE_ICONS, ...ADMIN_LUCIDE_ICONS),

    MessageService,
    ConfirmationService,
    { provide: PERMISSION_CHECKER, useExisting: PermissionService },
  ],
};
