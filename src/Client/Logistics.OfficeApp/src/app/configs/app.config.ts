import {environment} from 'src/environments/environment';

export const AppConfig = {
  apiHost: environment.apiHost,
  idHost: environment.idHost,
  mapboxToken: environment.mapboxToken,
  storage: {
    keys: {
      user: 'User',
    },
  },
};
