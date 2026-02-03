import { CommonModule } from "@angular/common";
import { Component, computed, effect, inject, signal } from "@angular/core";
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
import { type MenuItem, PanelMenu } from "../panel-menu";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { sidebarItems } from "./sidebar-items";

const ROLE_MENU_ACCESS: Record<string, string[] | "*"> = {
  [UserRole.Driver]: ["Home", "Messages"],
  [UserRole.Dispatcher]: ["Home", "Messages", "Operations", "Directory"],
  [UserRole.Manager]: [
    "Home",
    "Dashboard",
    "Messages",
    "Operations",
    "Fleet",
    "Safety",
    "Directory",
    "Accounting",
    "Reports",
  ],
  [UserRole.Owner]: "*",
};

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
  protected readonly navItems = signal<MenuItem[]>(sidebarItems as MenuItem[]);

  protected readonly profileMenuItems: PrimeMenuItem[] = [
    {
      label: "User name",
      icon: "pi pi-user",
      items: [
        { label: "Profile", command: () => this.openAccountUrl() },
        { separator: true },
        { label: "Sign out", command: () => this.logout() },
      ],
    },
  ];

  constructor() {
    // Update profile menu label when user data changes
    effect(() => {
      const fullName = this.userFullName();
      if (fullName) {
        this.profileMenuItems[0].label = fullName;
      }
    });

    // Refresh nav items when tenant/features change
    effect(() => {
      this.tenantService.tenantData(); // Track tenant changes
      this.featureProvider.refreshFeatures().then(() => {
        this.updateNavItems(this.authService.getUserData()?.role ?? null);
      });
    });

    // Update nav items when user role changes
    this.authService.onUserDataChanged().subscribe((userData) => {
      if (userData?.role) {
        this.updateNavItems(userData.role);
      }
    });
  }

  private updateNavItems(userRole: string | null): void {
    if (!userRole) {
      this.navItems.set([]);
      return;
    }

    let items = this.filterByFeature(sidebarItems as MenuItem[]);
    const allowedMenus = ROLE_MENU_ACCESS[userRole];

    if (allowedMenus === "*") {
      this.navItems.set(items);
      return;
    }

    if (allowedMenus) {
      items = items.filter((item) => item.label && allowedMenus.includes(item.label));

      // Dispatcher: only show Customers in Directory
      if (userRole === UserRole.Dispatcher) {
        items = items.map((item) =>
          item.label === "Directory" && item.items
            ? {
                ...item,
                items: item.items.filter((child) => (child as MenuItem).label === "Customers"),
              }
            : item,
        );
      }
    } else {
      items = [];
    }

    this.navItems.set(items);
  }

  private filterByFeature(items: MenuItem[]): MenuItem[] {
    return items
      .filter((item) => !item.feature || this.featureService.isEnabled(item.feature))
      .map((item) => {
        if (!item.items?.length) return item;

        const children = this.filterByFeature(item.items as MenuItem[]);
        const cleaned = this.cleanupSeparators(children);

        return cleaned.length === 0 && !item.route ? null : { ...item, items: cleaned };
      })
      .filter((item): item is MenuItem => item !== null);
  }

  private cleanupSeparators(items: MenuItem[]): MenuItem[] {
    return items.filter((item, i) => {
      const isSeparator = item.styleClass === "menu-separator" && item.disabled;
      if (!isSeparator) return true;

      return items
        .slice(i + 1)
        .some((next) => !(next.styleClass === "menu-separator" && next.disabled));
    });
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
