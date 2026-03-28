import { Component, computed, inject } from "@angular/core";
import { Router } from "@angular/router";
import { Converters } from "@logistics/shared";
import { PermissionService } from "@logistics/shared/services";
import { AvatarModule } from "primeng/avatar";
import { ButtonModule } from "primeng/button";
import { DividerModule } from "primeng/divider";
import { DrawerModule } from "primeng/drawer";
import { AuthService } from "@/core/auth";
import { LayoutService } from "@/core/services/layout.service";
import { type AdminNavSection, sidebarSections } from "../sidebar/sidebar-items";

@Component({
  selector: "adm-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  styleUrl: "./mobile-drawer.css",
  imports: [DrawerModule, ButtonModule, AvatarModule, DividerModule],
})
export class MobileDrawer {
  private readonly router = inject(Router);
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);
  private readonly permissionService = inject(PermissionService);

  protected readonly visible = this.layoutService.mobileMenuOpen;
  protected readonly userName = this.authService.userName;
  protected readonly userInitials = computed(() => Converters.getInitials(this.userName()));

  protected readonly sections = computed<AdminNavSection[]>(() => {
    return sidebarSections
      .map((section) => ({
        ...section,
        items: section.items.filter((item) => {
          if (!item.permission) return true;
          return this.permissionService.hasPermission(item.permission);
        }),
      }))
      .filter((section) => section.items.length > 0);
  });

  protected onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.layoutService.closeMobileMenu();
    }
  }

  protected navigateTo(route: string): void {
    this.router.navigateByUrl(route);
    this.layoutService.closeMobileMenu();
  }

  protected logout(): void {
    this.layoutService.closeMobileMenu();
    this.authService.logout();
  }
}
