import { Component, OnInit } from '@angular/core';
import { AppConfig } from '@configs';
import { User } from '@shared/models/user';
import { ApiService } from '@shared/services';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { MessageService } from 'primeng/api';
import { of, switchMap } from 'rxjs';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss']
})
export class TopbarComponent implements OnInit {
  isAuthenticated: boolean;
  isBusy: boolean;
  user?: User;
  tenantName: string;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.isAuthenticated = false;
    this.isBusy = false;
    this.tenantName = 'Company name';
  }
  
  ngOnInit(): void {
    this.oidcSecurityService.userData$.subscribe(({userData}) => {
      this.user = userData;
    });

    this.oidcSecurityService.isAuthenticated$.pipe(
      switchMap(({isAuthenticated}) => {
        this.isAuthenticated = isAuthenticated;
        
        if (isAuthenticated) {
          //this.oidcSecurityService.getAccessToken().subscribe(i => console.log(i));
          return this.apiService.getTenant();
        }

        return of({success: false, value: null});
      })
    ).subscribe(result => {
      if (result.success && result.value?.displayName) {
        this.tenantName = result.value?.displayName;
      }

      this.isBusy = false;
    });
  }

  login() {
    this.isBusy = true;
    this.oidcSecurityService.authorize();
  }

  logout() {
    this.oidcSecurityService.logoff();
  }

  openAccountUrl() {
    window.open(`${AppConfig.idHost}/account/manage/profile`, '_blank');
  }
}
