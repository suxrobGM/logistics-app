import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { type ApplicationConfig, provideZoneChangeDetection } from "@angular/core";
import { provideRouter } from "@angular/router";
import { provideApi } from "@logistics/shared";
import Aura from "@primeuix/themes/aura";
import { providePrimeNG } from "primeng/config";
import { environment } from "@/env";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([])),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: ".dark-mode",
        },
      },
    }),
    provideApi({ baseUrl: environment.apiUrl }),
  ],
};
