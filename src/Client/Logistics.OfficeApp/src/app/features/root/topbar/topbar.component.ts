import {Component, OnInit} from '@angular/core';
import {of, switchMap} from 'rxjs';
import {AppConfig} from '@configs';
import {ApiService} from '@core/services';
import {AuthService, UserData} from '@core/auth';


@Component({
  selector: 'app-topbar',
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit {
  isAuthenticated: boolean;
  isBusy: boolean;
  tenantName: string;
  userRole: string | null;
  user: UserData | null;

  constructor(
    private authService: AuthService,
    private apiService: ApiService)
  {
    this.isAuthenticated = false;
    this.isBusy = false;
    this.tenantName = '';
    this.user = null;
    this.userRole = null;
  }

  ngOnInit(): void {
    this.authService.onUserDataChanged().subscribe((userData) => {
      this.user = userData;
      this.userRole = this.authService.getUserRoleName();
    });

    this.authService.onAuthenticated().pipe(
        switchMap((isAuthenticated) => {
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
    this.authService.login();
  }

  logout() {
    this.authService.logout();
  }

  openAccountUrl() {
    window.open(`${AppConfig.idHost}/account/manage/profile`, '_blank');
  }
}
