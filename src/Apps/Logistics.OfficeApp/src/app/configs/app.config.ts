import { InjectionToken } from '@angular/core';

export const APP_CONFIG = new InjectionToken('app.config');

export const AppConfig = {
    apiHost: 'https://localhost:7000'
}