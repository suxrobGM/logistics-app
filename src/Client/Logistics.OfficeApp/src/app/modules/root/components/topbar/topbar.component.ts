import {Component, OnInit} from '@angular/core';
import {AppConfig} from '@core';
import {UserData} from '@shared/models';
import {ApiService, UserDataService} from '@shared/services';
import {getRoleName} from '@shared/types';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {of, switchMap} from 'rxjs';

@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit {
  isAuthenticated: boolean;
  isBusy: boolean;
  tenantName: string;
  userRole?: string;
  user: UserData | null;

  constructor(
    private apiService: ApiService,
    private oidcService: OidcSecurityService,
    private userDataService: UserDataService)
  {
    this.isAuthenticated = false;
    this.isBusy = false;
    this.tenantName = '';
    this.user = null;
  }

  ngOnInit(): void {
    this.oidcService.userData$.subscribe(({userData}) => {
      this.userDataService.setUser(userData);
      this.user = this.userDataService.getUser();
      this.userRole = getRoleName(this.user?.roles[0]);
    });

    this.oidcService.isAuthenticated$.pipe(
        switchMap(({isAuthenticated}) => {
          this.isAuthenticated = isAuthenticated;

          if (isAuthenticated) {
            return this.apiService.getTenant();
          }

          return of({success: false, value: null});
        }),
    ).subscribe((result) => {
      if (result.success && result.value?.displayName) {
        this.tenantName = result.value?.displayName;
      }

      this.isBusy = false;
    });
  }

  login() {
    this.isBusy = true;
    this.oidcService.authorize();
  }

  logout() {
    this.oidcService.logoff().subscribe((result) => result);
  }

  openAccountUrl() {
    window.open(`${AppConfig.idHost}/account/manage/profile`, '_blank');
  }
}
