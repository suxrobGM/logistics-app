import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { DrawerModule } from "primeng/drawer";
import { ButtonModule } from "primeng/button";
import { LayoutService } from "@/core/services/layout.service";
import { AuthService } from "@/core/auth";
import { TenantService } from "@/core/services";
import { environment } from "@/env";
import type { MenuItem } from "../panel-menu";
import { sidebarItems } from "../sidebar/sidebar-items";

interface MobileMenuItem {
  label: string;
  icon?: string;
  route?: string;
  items?: MobileMenuItem[];
  expanded?: boolean;
}

@Component({
  selector: "app-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  styleUrl: "./mobile-drawer.css",
  imports: [DrawerModule, ButtonModule],
})
export class MobileDrawer {
  private readonly router = inject(Router);
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);

  protected readonly visible = this.layoutService.mobileMenuOpen;
  protected readonly companyName = signal<string | null>(null);
  protected readonly userFullName = signal<string | null>(null);
  protected readonly userRole = signal<string | null>(null);
  protected readonly navItems = signal<MobileMenuItem[]>(this.transformMenuItems([...sidebarItems]));

  constructor() {
    this.authService.onUserDataChanged().subscribe((userData) => {
      if (userData?.getFullName()) {
        this.userFullName.set(userData.getFullName());
      }
      this.userRole.set(this.authService.getUserRoleName());
    });

    this.tenantService.tenantDataChanged$.subscribe((tenantData) => {
      if (tenantData?.subscription == null || !this.tenantService.isSubscriptionActive()) {
        this.navItems.set(
          this.transformMenuItems([...sidebarItems].filter((item) => item.label !== "Subscription")),
        );
      }
      this.companyName.set(tenantData?.companyName ?? null);
    });
  }

  protected onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.layoutService.closeMobileMenu();
    }
  }

  protected navigateTo(route: string): void {
    this.router.navigateByUrl(route);
    this.layoutService.closeMobileMenu();
  }

  protected toggleExpanded(item: MobileMenuItem): void {
    item.expanded = !item.expanded;
  }

  protected logout(): void {
    this.layoutService.closeMobileMenu();
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }

  private transformMenuItems(items: MenuItem[]): MobileMenuItem[] {
    return items
      .filter((item): item is MenuItem & { label: string } => !!item.label)
      .map((item) => ({
        label: item.label,
        icon: item.icon?.replace("text-3xl!", "text-lg"),
        route: item.route,
        items: item.items
          ?.filter((subItem): subItem is MenuItem & { label: string } => !!subItem.label)
          .map((subItem) => ({
            label: subItem.label,
            route: (subItem as MenuItem).route,
          })),
        expanded: false,
      }));
  }
}
