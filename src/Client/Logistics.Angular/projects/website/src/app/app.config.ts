import { provideBrowserGlobalErrorListeners, type ApplicationConfig } from "@angular/core";
import { provideClientHydration, withEventReplay } from "@angular/platform-browser";
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
  withRouterConfig,
} from "@angular/router";
import { provideSpartanHlm } from "@logistics/shared";
import { provideApi } from "@logistics/shared/api";
import { environment } from "../environments/environment";
import { routes } from "./app.routes";

export const appConfig: ApplicationConfig = {
  providers: [
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
