import { provideBrowserGlobalErrorListeners, type ApplicationConfig } from "@angular/core";
import { provideClientHydration, withEventReplay } from "@angular/platform-browser";
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
  withRouterConfig,
} from "@angular/router";
import { BASE_LUCIDE_ICONS } from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { provideLucideIcons } from "@lucide/angular";
import Aura from "@primeuix/themes/aura";
import { ConfirmationService, MessageService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import { environment } from "../environments/environment";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideLucideIcons(...BASE_LUCIDE_ICONS),
    provideBrowserGlobalErrorListeners(),
    provideRouter(
      routes,
      withInMemoryScrolling({
        scrollPositionRestoration: "enabled",
        anchorScrolling: "enabled",
      }),
      withComponentInputBinding(),
      // Angular 22 changed the default paramsInheritanceStrategy from 'emptyOnly' to 'always'.
      // Pin 'emptyOnly' so children don't silently inherit parent params/data (no migration exists).
      withRouterConfig({ paramsInheritanceStrategy: "emptyOnly" }),
    ),
    provideClientHydration(withEventReplay()),
    provideApi({
      baseUrl: environment.apiUrl,
    }),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false,
        },
      },
    }),

    // PrimeNG Services
    MessageService,
    ConfirmationService,
  ],
};
