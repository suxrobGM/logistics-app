import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {TooltipModule} from 'primeng/tooltip';


@Component({
  selector: 'app-sidebar',
  standalone: true,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  imports: [
    CommonModule,
    RouterLink,
    TooltipModule,
  ],
})
export class SidebarComponent {
  public isOpened = false;

  constructor() {}

  toggle() {
    this.isOpened = !this.isOpened;
  }
}
