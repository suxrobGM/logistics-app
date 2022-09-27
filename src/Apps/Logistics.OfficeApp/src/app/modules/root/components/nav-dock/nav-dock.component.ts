import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { UserRole } from '@shared/types';

@Component({
  selector: 'app-nav-dock',
  templateUrl: './nav-dock.component.html',
  styleUrls: ['./nav-dock.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class NavDockComponent implements OnInit {
  public dockItems: MenuItem[];

  constructor(private oidcService: OidcSecurityService) {
    this.dockItems = this.createMenuItems();
  }

  public ngOnInit() {
    this.oidcService.userData$.subscribe(({userData}) => {
      if (!userData?.role) {
        return;
      }

      const userRole = userData.role;
      const hasEnoughRole = userRole?.includes(UserRole.AppAdmin) || 
        userRole?.includes(UserRole.Owner) || 
        userRole?.includes(UserRole.Manager)

      if (hasEnoughRole) {
        this.dockItems = this.createMenuItems([{
          label: 'Report',
          icon: 'assets/icons/report.svg',
          link: 'report'
        }]);
      }
    });
  }

  private createMenuItems(additionalMenuItems?: MenuItem[]): MenuItem[] {
    const menuItems: MenuItem[] = [
      {
        label: 'Dashboard',
        icon: 'assets/icons/home.svg',
        link: 'dashboard'
      },
      {
        label: 'Loads',
        icon: 'assets/icons/delivery-container.svg',
        link: 'loads'
      },
      {
        label: 'Trucks',
        icon: 'assets/icons/delivery-truck.svg',
        link: 'trucks'
      },
      {
        label: 'Employees',
        icon: 'assets/icons/users.svg',
        link: 'employees'
      }
    ];

    additionalMenuItems?.forEach(i => menuItems.push(i));
    return menuItems;
  }
}

type MenuItem = {
  label: string;
  icon: string;
  link: string;
}
