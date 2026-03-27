import { Component, computed, effect, inject, signal } from "@angular/core";
import { UserRole } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import { PopoverModule } from "primeng/popover";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import {
  ChatService,
  CommandPaletteService,
  DispatchBadgeService,
  SidebarFavoritesService,
  TenantService,
  TmsFeatureProvider,
} from "@/core/services";
import { environment } from "@/env";
import { type NavItem, NavMenu, type NavSection } from "../nav-menu";
import { NotificationBell } from "../notification-bell";
import { ThemeToggle } from "../theme-toggle/theme-toggle";
import { FavoritesBar } from "./favorites-bar/favorites-bar";
import { sidebarSections } from "./sidebar-items";

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
  selector: "app-sidebar",
  templateUrl: "./sidebar.html",
  styleUrl: "./sidebar.css",
  imports: [TooltipModule, PopoverModule, NavMenu, FavoritesBar, ThemeToggle, NotificationBell],
})
export class Sidebar {
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly featureService = inject(FeatureService);
  private readonly featureProvider = inject(TmsFeatureProvider);
  private readonly chatService = inject(ChatService);
  private readonly dispatchBadgeService = inject(DispatchBadgeService);
  private readonly favoritesService = inject(SidebarFavoritesService);
  private readonly commandPaletteService = inject(CommandPaletteService);

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

  protected readonly filteredSections = signal<NavSection[]>([]);

  constructor() {
    // Refresh nav items when tenant/features change
    effect(() => {
      this.tenantService.tenantData();
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
    this.commandPaletteService.buildIndex(filtered);
  }

  private filterItems(items: NavItem[], allowedItems: string[] | "*"): NavItem[] {
    return items
      .filter((item) => {
        // Check role access
        if (allowedItems !== "*" && !allowedItems.includes(item.id)) {
          return false;
        }
        // Check feature flag
        if (item.feature && !this.featureService.isEnabled(item.feature)) {
          return false;
        }
        return true;
      })
      .map((item) => {
        if (!item.children) return item;
        // Filter children by feature
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

        if (item.id === "ai-dispatch") {
          this.dispatchBadgeService.refresh();
          item.badge = () => {
            const count = this.dispatchBadgeService.pendingCount();
            return count > 0 ? count : null;
          };
        }
      }
    }
    return sections;
  }

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
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }
}
