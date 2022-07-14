import { Component, OnInit } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Logistics.OfficeApp';
  isAuthenticated: boolean;

  constructor(public oidcSecurityService: OidcSecurityService) {
    this.isAuthenticated = false;
  }

  ngOnInit(): void {
    this.oidcSecurityService.checkAuth().subscribe(({ isAuthenticated, userData, accessToken}) => {
      this.isAuthenticated = isAuthenticated;
      //console.log(`Current access token is '${accessToken}'`);
      //console.log(userData);
    });
    
    this.oidcSecurityService.isAuthenticated$.subscribe(({isAuthenticated}) => {
      this.isAuthenticated = isAuthenticated;
    });
  }
}
