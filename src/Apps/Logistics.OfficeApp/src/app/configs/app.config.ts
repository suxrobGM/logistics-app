import { InjectionToken } from '@angular/core';

export const APP_CONFIG = new InjectionToken('app.config');

export const AppConfig = {
    apiHost: 'https://localhost:7000',
    mapboxToken: 'pk.eyJ1Ijoic3V4cm9iZ20iLCJhIjoiY2w0dmsyMGd1MDEzZDNjcXcwZGRtY255MSJ9.XwGTNZx_httMhW0Fu34udQ'
}