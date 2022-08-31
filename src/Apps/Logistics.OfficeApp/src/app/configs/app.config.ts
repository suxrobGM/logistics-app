import { InjectionToken } from '@angular/core';
import { environment } from 'src/environments/environment';

export const APP_CONFIG = new InjectionToken('app.config');

export const AppConfig = {
  apiHost: environment.apiHost,
  idHost: environment.idHost,
  mapboxToken: environment.mapboxToken
}