import { Component, inject } from "@angular/core";
import { toSignal } from "@angular/core/rxjs-interop";
import { NavigationEnd, Router, RouterLink } from "@angular/router";
import { LayoutService } from "@logistics/shared/services";
import { UiButton } from "@logistics/shared/ui";
import { filter, map, startWith } from "rxjs";
import { NotificationBell } from "../notification-bell";

@Component({
  selector: "app-mobile-header",
  templateUrl: "./mobile-header.html",
  imports: [NotificationBell, RouterLink, UiButton],
})
export class MobileHeader {
  private readonly layoutService = inject(LayoutService);
  private readonly router = inject(Router);

  protected readonly mobileMenuOpen = this.layoutService.mobileMenuOpen;
  protected readonly pageTitle = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => this.getPageTitle()),
      startWith(this.getPageTitle()),
    ),
  );

  protected toggleMenu(): void {
    this.layoutService.toggleMobileMenu();
  }

  private getPageTitle(): string {
    let route = this.router.routerState.root;
    while (route.firstChild) {
      route = route.firstChild;
    }
    return (route.snapshot.data["breadcrumb"] as string) ?? "TMS Portal";
  }
}
