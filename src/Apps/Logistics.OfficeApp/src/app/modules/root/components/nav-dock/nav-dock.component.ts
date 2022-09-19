import { Component, OnInit, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-nav-dock',
  templateUrl: './nav-dock.component.html',
  styleUrls: ['./nav-dock.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class NavDockComponent implements OnInit {
  dockItems!: MenuItem[];

  constructor() { }

  ngOnInit() {
    this.dockItems = [
      {
        label: 'Dashboard',
        icon: 'assets/icons/home.svg',
        link: 'dashboard',
      },
      {
        label: 'Loads',
        icon: 'assets/icons/delivery-container.svg',
        link: 'loads',
      },
      {
        label: 'Trucks',
        icon: 'assets/icons/delivery-truck.svg',
        link: 'trucks',
      },
      {
        label: 'Employees',
        icon: 'assets/icons/users.svg',
        link: 'employees',
      },
      {
        label: 'Report',
        icon: 'assets/icons/report.svg',
        link: 'report',
      }
    ];

  }

}

type MenuItem = {
  label: string;
  icon: string;
  link: string;
}
