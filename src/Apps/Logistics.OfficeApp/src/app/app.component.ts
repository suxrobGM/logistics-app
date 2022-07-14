import { Component } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Logistics.OfficeApp';
  isAuthenticated: boolean;

  constructor(public oidcSecurityService: OidcSecurityService) {
    this.isAuthenticated = false;

    // if (!this.isAuthenticated) {
    //   this.login();
    // }

    oidcSecurityService.isAuthenticated().subscribe(isAuthenticated => {
      this.isAuthenticated = isAuthenticated;
    });

    oidcSecurityService.checkAuth().subscribe(({ isAuthenticated, userData, accessToken}) => {
      console.log(`Is authenticated '${isAuthenticated}'`);
      console.log(`Current access token is '${accessToken}'`);
    });
  }
}
