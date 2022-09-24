import { InjectionToken } from '@angular/core';
import { environment } from 'src/environments/environment';

export const AppConfig = {
  apiHost: environment.apiHost,
  idHost: environment.idHost,
  mapboxToken: environment.mapboxToken
}