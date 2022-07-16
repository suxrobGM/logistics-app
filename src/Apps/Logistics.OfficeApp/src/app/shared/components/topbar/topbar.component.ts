import { AfterViewInit, Component, OnInit } from '@angular/core';
import { UserData } from '@app/shared/models/user-data';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})
export class TopbarComponent implements OnInit {
  isAuthenticated = false;
  user?: UserData;

  constructor(public oidcSecurityService: OidcSecurityService) {}
  
  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe(({userData}) => {
      this.user = userData;
    });

    this.oidcSecurityService.isAuthenticated$.subscribe(({isAuthenticated}) => {
      this.isAuthenticated = isAuthenticated;
    });
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  logout() {
    this.oidcSecurityService.logoff();
  }
}
