import {Component, signal} from "@angular/core";
import {RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {AuthService} from "@/core/auth";
import {BreadcrumbComponent, SidebarComponent} from "@/components/layout";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.scss",
  imports: [BreadcrumbComponent, ToastModule, RouterOutlet, SidebarComponent],
})
export class AppComponent {
  public readonly isAuthenticated = signal(false);

  constructor(
    private readonly authService: AuthService
    //private readonly router: Router
  ) {
    this.authService
      .checkAuth()
      .subscribe((isAuthenticated) => this.isAuthenticated.set(isAuthenticated));

    this.authService.onAuthenticated().subscribe((result) => this.isAuthenticated.set(result));
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
