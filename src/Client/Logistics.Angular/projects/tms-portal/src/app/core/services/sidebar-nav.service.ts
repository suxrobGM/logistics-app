import { computed, effect, inject, Injectable } from "@angular/core";
import { Permission, UserRole } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import { AuthService, PermissionService } from "@/core/auth";
import { passesAccessGate, type NavItem, type NavSection } from "@/shared/layout/nav-menu";
import { sidebarSections } from "@/shared/layout/sidebar/sidebar-items";
import { ChatService } from "./chat.service";
import { CommandPaletteService } from "./command-palette.service";
import { DispatchBadgeService } from "./dispatch-badge.service";
import { SidebarFavoritesService } from "./sidebar-favorites.service";
import { TenantService } from "./tenant.service";

/**
 * The only role not filtered by permission alone: a driver's permissions are shaped for the mobile
 * app and would open far more of the web portal than a driver should see.
 */
const DRIVER_NAV_ITEMS = ["home", "messages"];

/** Hidden in owner-operator mode. Timesheets is a `payroll` child, so it leaves with its parent. */
const SOLO_HIDDEN_ITEMS = ["employees", "payroll", "messages"];

/**
 * Single home for the sidebar/mobile-drawer nav pipeline: permission + feature filtering, badge
 * wiring, favorites init and the command-palette index. Both surfaces consume the same two
 * computeds, so they cannot drift apart.
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

  /** Nav item id -> the count signal its badge renders. Built once; `wireBadges` only reads it. */
  private readonly badgeSources: Record<string, () => number> = {
    messages: this.chatService.unreadCount,
    "ai-dispatch-assistant": this.dispatchBadgeService.pendingCount,
  };

  private readonly role = computed(() => this.authService.userData()?.role ?? null);

  /**
   * Permissions arrive after the user does, and until they land every `hasPermission` answers
   * false. Building before that yields a tree the favorites seed would persist as-is.
   */
  private readonly ready = computed(() => !!this.role() && this.permissionService.resolved());

  /** Hidden children intact. Fed to the command palette + favorites bar. */
  public readonly fullSections = computed<NavSection[]>(() => this.buildSections());

  /** Hidden children stripped. Consumed by `app-nav-menu`. */
  public readonly menuSections = computed<NavSection[]>(() =>
    this.fullSections()
      .map((section) => ({ ...section, items: this.stripHiddenChildren(section.items) }))
      .filter((section) => section.items.length > 0),
  );

  constructor() {
    effect(() => {
      if (!this.ready()) return;
      if (this.permissionService.hasPermission(Permission.Dispatch.View)) {
        this.dispatchBadgeService.refresh();
      }

      const sections = this.fullSections();
      this.favoritesService.initWithRole(this.role()!, this.collectItemIds(sections));
      this.commandPaletteService.buildIndex(sections);
    });
  }

  private buildSections(): NavSection[] {
    if (!this.ready()) return [];
    const role = this.role()!;

    return sidebarSections
      .map((section) => ({
        ...section,
        items: this.wireBadges(this.filterItems(section.items, role)),
      }))
      .filter((section) => section.items.length > 0);
  }

  private filterItems(items: NavItem[], role: string): NavItem[] {
    const isSolo = this.tenantService.isSoloMode();

    return items
      .filter((item) => {
        if (role === UserRole.Driver && !DRIVER_NAV_ITEMS.includes(item.id)) return false;
        if (isSolo && SOLO_HIDDEN_ITEMS.includes(item.id)) return false;
        return this.isVisible(item);
      })
      .map((item) => {
        if (!item.children) return item;

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
    return passesAccessGate(item, this.featureService, this.permissionService);
  }

  /** Derives the menu tree from {@link fullSections}, so the filter pipeline runs only once. */
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
   * An item with its own route collapses to a direct link once its children are gone; `undefined`
   * rather than `[]` because nav-menu branches on `@if (item.children)`. Routeless items are dropped.
   */
  private collapseOrKeep(item: NavItem, children: NavItem[]): NavItem | null {
    if (children.length === 0) {
      return item.route ? { ...item, children: undefined } : null;
    }
    return { ...item, children };
  }

  /**
   * Copies rather than mutates, so the static `sidebarSections` never needs cloning first. A group
   * with no source of its own sums its children, so a count stays visible while the group is shut.
   */
  private wireBadges(items: NavItem[]): NavItem[] {
    return items.map((item) => {
      const children = item.children ? this.wireBadges(item.children) : undefined;
      const source = this.badgeSources[item.id];

      if (source) {
        return { ...item, children, badge: () => source() || null };
      }

      if (children?.some((child) => child.badge)) {
        const badge = () =>
          children.reduce((total, child) => total + (child.badge?.() ?? 0), 0) || null;
        return { ...item, children, badge };
      }

      return children ? { ...item, children } : item;
    });
  }
}
