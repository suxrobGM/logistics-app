import { Component, inject } from "@angular/core";
import { LayoutService } from "@logistics/shared/services";
import { UiDrawer } from "@logistics/shared/ui";
import { BrandHeader } from "../brand-header/brand-header";
import { CopilotLauncher } from "../copilot-launcher/copilot-launcher";
import { NavMenu } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { SearchTrigger } from "../search-trigger/search-trigger";
import { FavoritesBar } from "../sidebar/favorites-bar/favorites-bar";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { UserMenu } from "../user-menu/user-menu";

@Component({
  selector: "app-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  imports: [
    BrandHeader,
    CopilotLauncher,
    FavoritesBar,
    NavMenu,
    NotificationBell,
    SearchTrigger,
    ThemeToggle,
    UiDrawer,
    UserMenu,
  ],
})
export class MobileDrawer {
  private readonly layoutService = inject(LayoutService);

  protected readonly visible = this.layoutService.mobileMenuOpen;

  protected onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.layoutService.closeMobileMenu();
    }
  }

  protected closeMenu(): void {
    this.layoutService.closeMobileMenu();
  }
}
