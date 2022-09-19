import { Component, OnInit } from '@angular/core';
import { Route, Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Logistics.OfficeApp';
  isAuthenticated: boolean;

  constructor(
    private oidcSecurityService: OidcSecurityService, 
    private router: Router) 
  {
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

    //this.printpath('', this.router.config);
  }

  printpath(parent: String, config: Route[]) {
    for (let i = 0; i < config.length; i++) {
      const route = config[i];
      console.log(parent + '/' + route.path);
      
      if (route.children) {
        const currentPath = route.path ? parent + '/' + route.path : parent;
        this.printpath(currentPath, route.children);
      }
    }
  }
}
