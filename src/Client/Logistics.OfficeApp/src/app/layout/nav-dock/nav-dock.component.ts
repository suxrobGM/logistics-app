import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {RouterLink} from '@angular/router';
import {TooltipModule} from 'primeng/tooltip';
import {SharedModule} from 'primeng/api';
import {DockModule} from 'primeng/dock';
import {Permissions} from '@core/helpers';
import {AuthService} from '@core/auth';


@Component({
  selector: 'app-nav-dock',
  templateUrl: './nav-dock.component.html',
  styleUrls: ['./nav-dock.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    DockModule,
    SharedModule,
    RouterLink,
    TooltipModule,
  ],
})
export class NavDockComponent implements OnInit {
  public dockItems: MenuItem[];

  constructor(private authService: AuthService) {
    this.dockItems = this.createMenuItems();
  }

  public ngOnInit() {
    this.authService.onUserDataChanged().subscribe((userData) => {
      const hasPermission = userData?.permissions.includes(Permissions.Report.View);

      if (hasPermission) {
        this.dockItems = this.createMenuItems([{
          label: 'Dashboard',
          icon: 'assets/icons/report.svg',
          link: 'dashboard',
        }]);
      }
    });
  }

  private createMenuItems(additionalMenuItems?: MenuItem[]): MenuItem[] {
    const menuItems: MenuItem[] = [
      {
        label: 'Home',
        icon: 'assets/icons/home.svg',
        link: 'home',
      },
      {
        label: 'Loads',
        icon: 'assets/icons/delivery-container.svg',
        link: 'load',
      },
      {
        label: 'Trucks',
        icon: 'assets/icons/delivery-truck.svg',
        link: 'truck',
      },
      {
        label: 'Employees',
        icon: 'assets/icons/users.svg',
        link: 'employee',
      },
    ];

    additionalMenuItems?.forEach((i) => menuItems.push(i));
    return menuItems;
  }
}

type MenuItem = {
  label: string;
  icon: string;
  link: string;
}
