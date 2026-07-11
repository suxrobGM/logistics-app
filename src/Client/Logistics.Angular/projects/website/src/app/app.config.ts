import { provideBrowserGlobalErrorListeners, type ApplicationConfig } from "@angular/core";
import { provideClientHydration, withEventReplay } from "@angular/platform-browser";
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
  withRouterConfig,
} from "@angular/router";
import { BASE_NG_ICONS, provideSpartanHlm } from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { provideIcons } from "@ng-icons/core";
import { WEBSITE_NG_ICONS } from "@/shared/icons/lucide-icons";
import { environment } from "../environments/environment";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
    provideIcons({ ...BASE_NG_ICONS, ...WEBSITE_NG_ICONS }),
    provideBrowserGlobalErrorListeners(),
    provideSpartanHlm(),
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
  ],
};
