import { Component, computed, inject, signal } from "@angular/core";
import { NavigationEnd, Router, RouterOutlet } from "@angular/router";
import { ConfirmDialog } from "primeng/confirmdialog";
import { ToastModule } from "primeng/toast";
import { filter } from "rxjs";
import { AuthService } from "@/core/auth";
import { Breadcrumb, Sidebar } from "@/shared/layout";

/** Routes that should not show the sidebar/breadcrumb layout */
const STANDALONE_ROUTES = ["/", "/unauthorized", "/404"];

@Component({
  selector: "app-root",
  templateUrl: "./app.html",
  imports: [Breadcrumb, ToastModule, RouterOutlet, Sidebar, ConfirmDialog],
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isAuthenticated = signal(false);
  private readonly currentUrl = signal("/");

  /** Show layout only when authenticated AND not on a standalone page */
  protected readonly showLayout = computed(
    () => this.isAuthenticated() && !STANDALONE_ROUTES.includes(this.currentUrl()),
  );

  constructor() {
    this.authService
      .checkAuth()
      .subscribe((isAuthenticated) => this.isAuthenticated.set(isAuthenticated));

    this.authService.onAuthenticated().subscribe((result) => this.isAuthenticated.set(result));

    // Track current URL to determine if we should show layout
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.currentUrl.set(event.urlAfterRedirects));
  }
}
