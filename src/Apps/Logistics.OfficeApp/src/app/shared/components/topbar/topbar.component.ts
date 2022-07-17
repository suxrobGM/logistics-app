import { Component, OnInit } from '@angular/core';
import { UserData } from '@app/shared/models/user-data';
import { ApiClientService } from '@app/shared/services/api-client.service';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { of, switchMap } from 'rxjs';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})
export class TopbarComponent implements OnInit {
  isAuthenticated = false;
  user?: UserData;
  tenantName = 'Company name';

  constructor(
    private oidcSecurityService: OidcSecurityService,
    private apiService: ApiClientService) 
  {
  }
  
  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe(({userData}) => {
      this.user = userData;
    });

    this.oidcSecurityService.isAuthenticated$.pipe(
      switchMap(({isAuthenticated}) => {
        this.isAuthenticated = isAuthenticated;
        
        if (isAuthenticated) {
          return this.apiService.getTenant();
        }

        return of({success: false, value: null});
      })
    ).subscribe(result => {
      if (result.success && result.value?.displayName) {
        this.tenantName = result.value?.displayName;
      }
    });
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  logout() {
    this.oidcSecurityService.logoff();
  }
}
