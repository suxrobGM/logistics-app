import { InjectionToken } from '@angular/core';

export const ROUTES_CONFIG = new InjectionToken('routes.config');

const routesNames = {
  dashboard: 'dashboard',
  home: 'home',
  error404: '404'
};

export const RoutesConfig: any = {
  routesNames,
  routes: {
    dashbaord: `/${routesNames.dashboard}`,
    home: `/${routesNames.home}`,
    error404: `/${routesNames.error404}`,
  },
}