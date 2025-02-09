import {Component, OnInit, signal} from "@angular/core";
import {Router, RouterOutlet} from "@angular/router";
import {ToastModule} from "primeng/toast";
import {AuthService} from "@/core/auth";
import {BreadcrumbComponent, SidebarComponent} from "@/components/layout";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
  standalone: true,
  imports: [BreadcrumbComponent, ToastModule, RouterOutlet, SidebarComponent],
})
export class AppComponent implements OnInit {
  public readonly isAuthenticated = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.authService
      .checkAuth()
      .subscribe((isAuthenticated) => this.isAuthenticated.set(isAuthenticated));

    this.authService.onAuthenticated().subscribe((result) => this.isAuthenticated.set(result));
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
