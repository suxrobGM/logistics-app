import {Component, OnInit} from '@angular/core';
import {Route, Router} from '@angular/router';
import {AuthService} from '@core/auth';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
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

    // this.printpath('', this.router.config);
    this.authService.onAuthenticated().subscribe((result) => this.isAuthenticated = result);
  }

  printpath(parent: String, config: Route[]) {
    for (let i = 0; i < config.length; i++) {
      const route = config[i];
      console.log(parent + '/' + route.path);

      if (route.children) {
        const currentPath = route.path ? parent + '/' + route.path : parent;
        this.printpath(currentPath, route.children);
      }
    }
  }
}
