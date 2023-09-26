import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TooltipModule} from 'primeng/tooltip';
import {SplitButtonModule} from 'primeng/splitbutton';
// import {MenuModule} from 'primeng/menu';
import {MenuItem} from 'primeng/api';
import {AppConfig} from '@configs';
import {AuthService} from '@core/auth';
import {ApiService, TenantService} from '@core/services';


@Component({
  selector: 'app-sidebar',
  standalone: true,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  imports: [
    CommonModule,
    RouterLink,
    TooltipModule,
    SplitButtonModule,
    // MenuModule,
  ],
})
export class SidebarComponent {
  public isAuthenticated: boolean;
  public isLoading: boolean;
  public isOpened: boolean;
  public companyName?: string;
  public userRole?: string;
  public userFullName?: string;
  public accountMenuItems: MenuItem[];

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private tenantService: TenantService)
  {
    this.isAuthenticated = false;
    this.isOpened = false;
    this.isLoading = false;
    this.accountMenuItems = [
      {
        label: 'Profile',
        command: () => this.openAccountUrl(),
      },
      {
        separator: true,
      },
      {
        label: 'Sign out',
        command: () => this.logout(),
      },
    ];
  }

  ngOnInit(): void {
    this.authService.onUserDataChanged().subscribe((userData) => {
      this.userFullName = userData?.getFullName();
      this.userRole = this.authService.getUserRoleName()!;
      this.fetchTenantData();
    });
  }

  private fetchTenantData() {
    this.apiService.getTenant().subscribe((result) => {
      if (!result.success) {
        return;
      }

      this.tenantService.setTenantData(result.value!);
      this.companyName = result.value!.displayName;
    });
  }

  toggle() {
    this.isOpened = !this.isOpened;
  }

  logout() {
    this.authService.logout();
  }

  openAccountUrl() {
    window.open(`${AppConfig.idHost}/account/manage/profile`, '_blank');
  }
}
