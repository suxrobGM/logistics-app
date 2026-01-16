import { Component, inject, signal } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ConfirmDialog } from "primeng/confirmdialog";
import { ToastModule } from "primeng/toast";
import { AuthService } from "@/core/auth";
import { Breadcrumb, Sidebar } from "@/shared/layout";

@Component({
  selector: "app-root",
  templateUrl: "./app.html",
  imports: [Breadcrumb, ToastModule, RouterOutlet, Sidebar, ConfirmDialog],
})
export class App {
  protected readonly isAuthenticated = signal(false);
  private readonly authService = inject(AuthService);

  constructor() {
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
