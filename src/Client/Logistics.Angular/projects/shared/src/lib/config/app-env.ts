declare const NG_MAPBOX_TOKEN: string | undefined;

/** Dev environment for every app; production builds swap in `app-env.prod.ts` via `fileReplacements`. */
export const environment = {
  production: false,
  apiUrl: "http://localhost:7000",
  identityServerUrl: "http://localhost:7001",
  // `bun start:tms` forwards .env's MAPBOX_TOKEN through `ng serve --define`; the other apps
  // never define it and never read the field.
  mapboxToken: typeof NG_MAPBOX_TOKEN === "string" ? NG_MAPBOX_TOKEN : "",
};
