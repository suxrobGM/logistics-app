import { Component, computed, inject, signal } from "@angular/core";
import { openIdentityAccountPage } from "@logistics/shared/auth";
import { Icon, UiPopover, UiTooltip } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import {
  CommandPaletteService,
  SidebarFavoritesService,
  SidebarNavService,
  TenantService,
} from "@/core/services";
import { environment } from "@/env";
import { NavMenu, type NavItem } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { FavoritesBar } from "./favorites-bar/favorites-bar";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.html",
  styleUrl: "./sidebar.css",
  imports: [FavoritesBar, Icon, NavMenu, NotificationBell, UiPopover, ThemeToggle, UiTooltip],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly favoritesService = inject(SidebarFavoritesService);
  private readonly commandPaletteService = inject(CommandPaletteService);
  private readonly sidebarNavService = inject(SidebarNavService);

  protected readonly isOpened = signal(true);
  protected readonly companyName = computed(
    () => this.tenantService.tenantData()?.companyName ?? null,
  );
  protected readonly companyLogoUrl = computed(
    () => this.tenantService.tenantData()?.logoUrl ?? null,
  );
  protected readonly userRole = computed(() => this.authService.getUserRoleName());
  protected readonly userFullName = computed(
    () => this.authService.getUserData()?.getFullName() ?? null,
  );

  /** Rendered menu (hidden children stripped). */
  protected readonly menuSections = this.sidebarNavService.menuSections;
  /** Full tree (hidden children intact) - drives the favorites bar. */
  protected readonly fullSections = this.sidebarNavService.fullSections;

  protected openCommandPalette(): void {
    this.commandPaletteService.open();
  }

  protected onNavContextMenu({ item }: { event: MouseEvent; item: NavItem }): void {
    // Toggle favorite on right-click
    if (this.favoritesService.isFavorite(item.id)) {
      this.favoritesService.remove(item.id);
    } else {
      this.favoritesService.add(item.id);
    }
  }

  protected toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    openIdentityAccountPage(environment.identityServerUrl, "profile");
  }

  protected openPrivacyUrl(): void {
    openIdentityAccountPage(environment.identityServerUrl, "privacy");
  }
}
