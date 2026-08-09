import { computed, effect, inject, Injectable } from "@angular/core";
import { UserRole } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import { AuthService, PermissionService } from "@/core/auth";
import { isNavItemVisible, type NavItem, type NavSection } from "@/shared/layout/nav-menu";
import { sidebarSections } from "@/shared/layout/sidebar/sidebar-items";
import { ChatService } from "./chat.service";
import { CommandPaletteService } from "./command-palette.service";
import { DispatchBadgeService } from "./dispatch-badge.service";
import { SidebarFavoritesService } from "./sidebar-favorites.service";
import { TenantService } from "./tenant.service";

/**
 * Which top-level nav item ids each role may see. `"*"` means every item.
 * Feature flags filter further on top of this.
 */
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
    "dvir",
    "safety",
    "employees",
    "customers",
    "payroll",
    "invoicing",
    "expenses",
    "reports",
    // Billing + Integrations only; per-route `data.permission` keeps Manager out of the rest.
    "settings",
  ],
  [UserRole.Owner]: "*",
};

/** Hidden in owner-operator mode. Timesheets is a `payroll` child, so it leaves with its parent. */
const SOLO_HIDDEN_ITEMS = ["employees", "payroll", "messages"];

/**
 * Single home for the sidebar/mobile-drawer nav pipeline: role + feature filtering, badge wiring,
 * favorites init and the command-palette index. Both surfaces consume the same two computeds so they
 * can never drift apart again.
 *
 * - {@link menuSections} - what the rendered menus show (`menuHidden` children stripped).
 * - {@link fullSections} - the full tree (hidden children intact) for the command palette + favorites.
 */
@Injectable({ providedIn: "root" })
export class SidebarNavService {
  private readonly authService = inject(AuthService);
  private readonly featureService = inject(FeatureService);
  private readonly permissionService = inject(PermissionService);
  private readonly tenantService = inject(TenantService);
  private readonly chatService = inject(ChatService);
  private readonly dispatchBadgeService = inject(DispatchBadgeService);
  private readonly favoritesService = inject(SidebarFavoritesService);
  private readonly commandPaletteService = inject(CommandPaletteService);

  private readonly role = computed(() => this.authService.userData()?.role ?? null);

  /** Role + feature filtered, hidden children intact. Fed to the palette index + favorites bar. */
  public readonly fullSections = computed<NavSection[]>(() => this.buildSections());

  /** Role + feature filtered, hidden children stripped. Consumed by `app-nav-menu`. */
  public readonly menuSections = computed<NavSection[]>(() =>
    this.fullSections()
      .map((section) => ({ ...section, items: this.stripHiddenChildren(section.items) }))
      .filter((section) => section.items.length > 0),
  );

  constructor() {
    // Favorites defaults + command-palette index track the full (hidden-inclusive) tree.
    effect(() => {
      const role = this.role();
      if (!role) return;
      this.dispatchBadgeService.refresh();
      const sections = this.fullSections();
      this.favoritesService.initWithRole(role, this.collectItemIds(sections));
      this.commandPaletteService.buildIndex(sections);
    });
  }

  private buildSections(): NavSection[] {
    const role = this.role();
    if (!role) return [];

    const allowedItems = ROLE_ITEM_ACCESS[role];

    return this.wireBadges(sidebarSections)
      .map((section) => ({
        ...section,
        items: this.filterItems(section.items, allowedItems),
      }))
      .filter((section) => section.items.length > 0);
  }

  private filterItems(items: NavItem[], allowedItems: string[] | "*"): NavItem[] {
    // Read here rather than in the constructor effect, so a mode change repaints the nav without
    // waiting on a feature refresh.
    const isSolo = this.tenantService.isSoloMode();

    return items
      .filter((item) => {
        // Role access.
        if (allowedItems !== "*" && !allowedItems.includes(item.id)) return false;
        if (isSolo && SOLO_HIDDEN_ITEMS.includes(item.id)) return false;
        // Feature flag + permission.
        if (!this.isVisible(item)) return false;
        return true;
      })
      .map((item) => {
        if (!item.children) return item;

        // Children filter on their own feature/permission; `menuHidden` is applied later per surface.
        const children = item.children.filter((child) => this.isVisible(child));
        return this.collapseOrKeep(item, children);
      })
      .filter((item): item is NavItem => item !== null);
  }

  private collectItemIds(sections: NavSection[]): string[] {
    return sections.flatMap((section) =>
      section.items.flatMap((item) => [item.id, ...(item.children?.map((c) => c.id) ?? [])]),
    );
  }

  private isVisible(item: NavItem): boolean {
    return isNavItemVisible(item, this.featureService, this.permissionService);
  }

  /**
   * Derives the rendered-menu tree from {@link fullSections} by dropping `menuHidden` children,
   * so the full pipeline (role/feature filter + badge wiring) runs only once.
   */
  private stripHiddenChildren(items: NavItem[]): NavItem[] {
    return items
      .map((item) => {
        if (!item.children) return item;
        const children = item.children.filter((child) => !child.menuHidden);
        return children.length === item.children.length
          ? item
          : this.collapseOrKeep(item, children);
      })
      .filter((item): item is NavItem => item !== null);
  }

  /**
   * An item with its own route (reports, settings) collapses to a direct link once its children are
   * gone - `children: undefined` so nav-menu renders `@if (item.children)` false. A childless item
   * with no route is dropped.
   */
  private collapseOrKeep(item: NavItem, children: NavItem[]): NavItem | null {
    if (children.length === 0) {
      return item.route ? { ...item, children: undefined } : null;
    }
    return { ...item, children };
  }

  /** Copies rather than mutates, so the static `sidebarSections` never needs cloning first. */
  private wireBadges(sections: readonly NavSection[]): NavSection[] {
    return sections.map((section) => ({
      ...section,
      items: section.items.map((item) => {
        const badge = this.badgeFor(item.id);
        return badge ? { ...item, badge } : item;
      }),
    }));
  }

  private badgeFor(itemId: string): (() => number | null) | null {
    const source = {
      messages: this.chatService.unreadCount,
      "ai-dispatch": this.dispatchBadgeService.pendingCount,
    }[itemId];

    return source ? () => source() || null : null;
  }
}
