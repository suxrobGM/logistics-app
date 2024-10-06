import {environment} from "src/environments/environment";

export const GLOBAL_CONFIG = {
  apiHost: environment.apiHost,
  idHost: environment.idHost,
  mapboxToken: environment.mapboxToken,
  storage: {
    keys: {
      user: "User",
    },
  },
};
