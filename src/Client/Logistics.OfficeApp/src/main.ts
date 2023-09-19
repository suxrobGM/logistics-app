import {enableProdMode, importProvidersFrom} from '@angular/core';
import {provideRouter} from '@angular/router';
import {withInterceptorsFromDi, provideHttpClient} from '@angular/common/http';
import {provideAnimations} from '@angular/platform-browser/animations';
import {BrowserModule, bootstrapApplication} from '@angular/platform-browser';
import {NgxMapboxGLModule} from 'ngx-mapbox-gl';
import {AppConfig} from '@configs';
import {CoreModule} from '@core/core.module';
import {APP_ROUTES} from './app/app.routes';
import {AppComponent} from './app/app.component';
import {environment} from './environments/environment';


if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(APP_ROUTES),
    importProvidersFrom(BrowserModule, CoreModule, NgxMapboxGLModule.withConfig({accessToken: AppConfig.mapboxToken})),
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
  ],
}).catch((err) => console.error(err));
