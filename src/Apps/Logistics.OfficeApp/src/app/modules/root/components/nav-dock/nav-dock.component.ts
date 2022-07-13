import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav-dock',
  templateUrl: './nav-dock.component.html',
  styleUrls: ['./nav-dock.component.scss']
})
export class NavDockComponent implements OnInit {
  dockItems!: MenuItem[];

  constructor() { }

  ngOnInit() {
    this.dockItems = [
      {
        label: 'Finder',
        icon: 'assets/icons/finder.svg'
      },
      {
        label: 'App Store',
        icon: 'assets/icons/appstore.svg'
      },
      {
        label: 'Safari',
        icon: 'assets/icons/safari.svg'
      },
      {
        label: 'Terminal',
        icon: 'assets/icons/terminal.svg'
      }
    ];

  }

}

type MenuItem = {
  label: string;
  icon: string;
}
