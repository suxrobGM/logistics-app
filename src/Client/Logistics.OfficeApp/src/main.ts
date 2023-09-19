import {enableProdMode, importProvidersFrom} from '@angular/core';
import {environment} from './environments/environment';
import {AppComponent} from './app/app.component';
import {SharedModule} from '@shared/shared.module';
import {RootModule} from '@modules/root';
import {CoreModule} from './app/core/core.module';
import {AppRoutingModule} from './app/app-routing.module';
import {withInterceptorsFromDi, provideHttpClient} from '@angular/common/http';
import {provideAnimations} from '@angular/platform-browser/animations';
import {BrowserModule, bootstrapApplication} from '@angular/platform-browser';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(BrowserModule, AppRoutingModule, CoreModule, RootModule, SharedModule),
    provideAnimations(),
    provideHttpClient(withInterceptorsFromDi()),
  ],
})
    .catch((err) => console.error(err));
