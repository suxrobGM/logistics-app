import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import type { MenuItem as PrimeMenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { PanelMenuModule } from "primeng/panelmenu";
import { SplitButtonModule } from "primeng/splitbutton";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import { TenantService, TmsFeatureProvider } from "@/core/services";
import { environment } from "@/env";
import { NotificationBell } from "../notification-bell";
import { PanelMenu, type MenuItem } from "../panel-menu";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { sidebarItems } from "./sidebar-items";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.html",
  styleUrl: "./sidebar.css",
  imports: [
    CommonModule,
    TooltipModule,
    ButtonModule,
    SplitButtonModule,
    PanelMenuModule,
    PanelMenu,
    ThemeToggle,
    NotificationBell,
  ],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly featureService = inject(FeatureService);
  private readonly featureProvider = inject(TmsFeatureProvider);

  public readonly isOpened = signal(true);
  public readonly companyName = signal<string | null>(null);
  public readonly companyLogoUrl = signal<string | null>(null);
  public readonly userRole = signal<string | null>(null);
  public readonly userFullName = signal<string | null>(null);
  public readonly navItems = signal<MenuItem[]>([]);
  public readonly profileMenuItems: PrimeMenuItem[];

  constructor() {
    this.profileMenuItems = [
      {
        label: "User name",
        icon: "pi pi-user",
        items: [
          {
            label: "Profile",
            command: () => this.openAccountUrl(),
          },
          {
            separator: true,
          },
          {
            label: "Sign out",
            command: () => this.logout(),
          },
        ],
      },
    ];

    this.authService.onUserDataChanged().subscribe((userData) => {
      if (!userData) {
        return; // Wait until user data is available before processing
      }

      if (userData.getFullName()) {
        this.userFullName.set(userData.getFullName());
        this.profileMenuItems[0].label = userData.getFullName();
      }

      const userRole = userData.role;
      this.userRole.set(this.authService.getUserRoleName());

      // Refresh nav items with role and feature filtering
      this.updateNavItems(userRole);
    });

    this.tenantService.tenantDataChanged$.subscribe((tenantData) => {
      this.companyName.set(tenantData?.companyName ?? null);
      this.companyLogoUrl.set(tenantData?.logoUrl ?? null);

      // Refresh features when tenant data changes
      this.featureProvider.refreshFeatures().then(() => {
        this.updateNavItems(this.authService.getUserData()?.role ?? null);
      });
    });

    // Initial nav items (before features are loaded)
    this.navItems.set(sidebarItems as MenuItem[]);
  }

  private updateNavItems(userRole: string | null): void {
    let filteredItems = this.filterMenuItemsByFeature(sidebarItems as MenuItem[]);

    // Settings menu is only visible to Owner role
    if (userRole !== UserRole.Owner) {
      filteredItems = filteredItems.filter((item) => item.label !== "Settings");
    }

    this.navItems.set(filteredItems);
  }

  /**
   * Recursively filters menu items based on feature availability.
   * Items with disabled features are removed.
   * Parent items are kept only if they have visible children after filtering.
   */
  private filterMenuItemsByFeature(items: MenuItem[]): MenuItem[] {
    return items
      .filter((item) => {
        // If item has a feature requirement, check if it's enabled
        if (item.feature && !this.featureService.isEnabled(item.feature)) {
          return false;
        }
        return true;
      })
      .map((item) => {
        // Recursively filter children
        if (item.items && item.items.length > 0) {
          const filteredChildren = this.filterMenuItemsByFeature(item.items as MenuItem[]);

          // Filter out separator items that have no content following them
          const cleanedChildren = this.cleanupSeparators(filteredChildren);

          // If all children are filtered out, hide the parent too (unless it has its own route)
          if (cleanedChildren.length === 0 && !item.route) {
            return null;
          }

          return { ...item, items: cleanedChildren };
        }
        return item;
      })
      .filter((item): item is MenuItem => item !== null);
  }

  /**
   * Removes separator items that don't have valid content following them.
   */
  private cleanupSeparators(items: MenuItem[]): MenuItem[] {
    const result: MenuItem[] = [];

    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      const isSeparator = item.styleClass === "menu-separator" && item.disabled;

      if (isSeparator) {
        // Check if there are any non-separator items following this one
        const hasFollowingContent = items.slice(i + 1).some(
          (nextItem) => !(nextItem.styleClass === "menu-separator" && nextItem.disabled)
        );

        if (hasFollowingContent) {
          result.push(item);
        }
      } else {
        result.push(item);
      }
    }

    return result;
  }

  protected toggle(): void {
    this.isOpened.set(!this.isOpened());
  }

  protected logout(): void {
    this.authService.logout();
  }

  protected openAccountUrl(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }
}
