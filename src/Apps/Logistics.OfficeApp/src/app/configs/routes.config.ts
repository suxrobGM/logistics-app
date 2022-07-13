import { InjectionToken } from '@angular/core';

export const ROUTES_CONFIG = new InjectionToken('routes.config');

const routesNames = {
  dashboard: 'dashboard',
  error404: '404'
};

export const RoutesConfig: any = {
  routesNames,
  routes: {
    dashboard: `/${routesNames.dashboard}`,
    error404: `/${routesNames.error404}`,
  },
}