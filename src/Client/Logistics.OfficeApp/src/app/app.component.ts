import {Component, OnInit} from '@angular/core';
import {NgIf} from '@angular/common';
import {Router, RouterOutlet} from '@angular/router';
import {ToastModule} from 'primeng/toast';
import {AuthService} from '@core/auth';
import {BreadcrumbComponent, SidebarComponent} from '@layout';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [
    NgIf,
    BreadcrumbComponent,
    ToastModule,
    RouterOutlet,
    SidebarComponent,
  ],
})
export class AppComponent implements OnInit {
  isAuthenticated: boolean;

  constructor(
    private authService: AuthService,
    private router: Router)
  {
    this.isAuthenticated = false;
  }

  ngOnInit(): void {
    this.authService.checkAuth().subscribe((isAuthenticated) => this.isAuthenticated = isAuthenticated);
    this.authService.onAuthenticated().subscribe((result) => this.isAuthenticated = result);
    // this.printPath('', this.router.config);
  }

  // private printPath(parent: String, config: Route[]) {
  //   for (let i = 0; i < config.length; i++) {
  //     const route = config[i];
  //     console.log(parent + '/' + route.path);

  //     if (route.children) {
  //       const currentPath = route.path ? parent + '/' + route.path : parent;
  //       this.printPath(currentPath, route.children);
  //     }
  //   }
  // }
}
