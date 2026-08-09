import { Component, computed, inject } from "@angular/core";
import { LayoutService } from "@logistics/shared/services";
import { Icon, UiTooltip } from "@logistics/shared/ui";
import { BrandHeader } from "../brand-header/brand-header";
import { CopilotLauncher } from "../copilot-launcher/copilot-launcher";
import { NavMenu } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { SearchTrigger } from "../search-trigger/search-trigger";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { UserMenu } from "../user-menu/user-menu";
import { FavoritesBar } from "./favorites-bar/favorites-bar";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.html",
  styleUrl: "./sidebar.css",
  imports: [
    BrandHeader,
    CopilotLauncher,
    FavoritesBar,
    Icon,
    NavMenu,
    NotificationBell,
    SearchTrigger,
    ThemeToggle,
    UiTooltip,
    UserMenu,
  ],
})
export class Sidebar {
  private readonly layoutService = inject(LayoutService);

  protected readonly collapsed = this.layoutService.navRailCollapsed;
  protected readonly isOpened = computed(() => !this.collapsed());

  protected toggle(): void {
    this.layoutService.toggleNavRail();
  }
}
