import { Component, computed, effect, inject, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import { DrawerModule } from "primeng/drawer";
import { PopoverModule } from "primeng/popover";
import { AuthService } from "@/core/auth";
import {
  ChatService,
  CommandPaletteService,
  SidebarFavoritesService,
  TenantService,
  TmsFeatureProvider,
} from "@/core/services";
import { LayoutService } from "@/core/services/layout.service";
import { environment } from "@/env";
import { type NavItem, NavMenu, type NavSection } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { FavoritesBar } from "../sidebar/favorites-bar/favorites-bar";
import { sidebarSections } from "../sidebar/sidebar-items";
import { ThemeToggle } from "../theme-toggle/theme-toggle";

const ROLE_ITEM_ACCESS: Record<string, string[] | "*"> = {
  [UserRole.Driver]: ["home", "messages"],
  [UserRole.Dispatcher]: ["home", "messages", "loads", "trips", "loadboard", "customers"],
  [UserRole.Manager]: [
    "home",
    "messages",
    "loads",
    "trips",
    "loadboard",
    "trucks",
    "eld",
    "maintenance",
    "safety",
    "employees",
    "customers",
    "payroll",
    "invoicing",
    "expenses",
    "reports",
  ],
  [UserRole.Owner]: "*",
};

@Component({
  selector: "app-mobile-drawer",
  templateUrl: "./mobile-drawer.html",
  styleUrl: "./mobile-drawer.css",
  imports: [DrawerModule, PopoverModule, NavMenu, NotificationBell, FavoritesBar, ThemeToggle],
})
export class MobileDrawer {
  private readonly layoutService = inject(LayoutService);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly featureService = inject(FeatureService);
  private readonly featureProvider = inject(TmsFeatureProvider);
  private readonly chatService = inject(ChatService);
  private readonly favoritesService = inject(SidebarFavoritesService);
  private readonly commandPaletteService = inject(CommandPaletteService);

  protected readonly visible = this.layoutService.mobileMenuOpen;
  protected readonly companyName = computed(
    () => this.tenantService.tenantData()?.companyName ?? null,
  );
  protected readonly companyLogoUrl = computed(
    () => this.tenantService.tenantData()?.logoUrl ?? null,
  );
  protected readonly userFullName = signal<string | null>(null);
  protected readonly userRole = signal<string | null>(null);
  protected readonly filteredSections = signal<NavSection[]>([]);

  constructor() {
    this.authService.onUserDataChanged().subscribe((userData) => {
      if (userData?.getFullName()) {
        this.userFullName.set(userData.getFullName());
      }
      this.userRole.set(this.authService.getUserRoleName());
      if (userData?.role) {
        this.updateNavItems(userData.role);
      }
    });

    effect(() => {
      this.tenantService.tenantData();
      this.featureProvider.refreshFeatures().then(() => {
        this.updateNavItems(this.authService.getUserData()?.role ?? null);
      });
    });
  }

  private updateNavItems(userRole: string | null): void {
    if (!userRole) {
      this.filteredSections.set([]);
      return;
    }

    this.favoritesService.initWithRole(userRole);
    const allowedItems = ROLE_ITEM_ACCESS[userRole];
    const sections = this.wireBadges(structuredClone(sidebarSections));

    const filtered = sections
      .map((section) => ({
        ...section,
        items: this.filterItems(section.items, allowedItems),
      }))
      .filter((section) => section.items.length > 0);

    this.filteredSections.set(filtered);
  }

  private filterItems(items: NavItem[], allowedItems: string[] | "*"): NavItem[] {
    return items
      .filter((item) => {
        if (allowedItems !== "*" && !allowedItems.includes(item.id)) return false;
        if (item.feature && !this.featureService.isEnabled(item.feature)) return false;
        return true;
      })
      .map((item) => {
        if (!item.children) return item;
        const children = item.children.filter(
          (child) => !child.feature || this.featureService.isEnabled(child.feature),
        );
        return children.length > 0 ? { ...item, children } : null;
      })
      .filter((item): item is NavItem => item !== null);
  }

  private wireBadges(sections: NavSection[]): NavSection[] {
    for (const section of sections) {
      for (const item of section.items) {
        if (item.id === "messages") {
          item.badge = () => {
            const count = this.chatService.unreadCount();
            return count > 0 ? count : null;
          };
        }
      }
    }
    return sections;
  }

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
