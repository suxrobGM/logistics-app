import { Component, computed, inject } from "@angular/core";
import { LayoutService } from "@logistics/shared/services";
import { Icon, UiDrawer, UiPopover } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { CommandPaletteService, SidebarNavService, TenantService } from "@/core/services";
import { environment } from "@/env";
import { NavMenu } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { FavoritesBar } from "../sidebar/favorites-bar/favorites-bar";
import { ThemeToggle } from "../theme-toggle/theme-toggle";

@Component({
  selector: "app-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  styleUrl: "./mobile-drawer.css",
  imports: [UiDrawer, FavoritesBar, Icon, NavMenu, NotificationBell, UiPopover, ThemeToggle],
})
export class MobileDrawer {
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly commandPaletteService = inject(CommandPaletteService);
  private readonly sidebarNavService = inject(SidebarNavService);

  protected readonly visible = this.layoutService.mobileMenuOpen;
  protected readonly companyName = computed(
    () => this.tenantService.tenantData()?.companyName ?? null,
  );
  protected readonly companyLogoUrl = computed(
    () => this.tenantService.tenantData()?.logoUrl ?? null,
  );
  protected readonly userFullName = computed(
    () => this.authService.getUserData()?.getFullName() ?? null,
  );
  protected readonly userRole = computed(() => this.authService.getUserRoleName());

  /** Rendered menu (hidden children stripped). */
  protected readonly menuSections = this.sidebarNavService.menuSections;
  /** Full tree (hidden children intact) - drives the favorites bar. */
  protected readonly fullSections = this.sidebarNavService.fullSections;

  protected onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.layoutService.closeMobileMenu();
    }
  }

  protected onNavItemClick(): void {
    this.layoutService.closeMobileMenu();
  }

  protected onFavoriteNavigate(): void {
    this.layoutService.closeMobileMenu();
  }

  protected openSearch(): void {
    this.layoutService.closeMobileMenu();
    this.commandPaletteService.open();
  }

  protected logout(): void {
    this.layoutService.closeMobileMenu();
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }
}
