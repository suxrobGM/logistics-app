import { NgTemplateOutlet } from "@angular/common";
import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { toSignal } from "@angular/core/rxjs-interop";
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from "@angular/router";
import { CountBadge, Icon, UiTooltip } from "@logistics/shared/ui";
import { filter } from "rxjs";
import { SidebarFavoritesService, SidebarNavService } from "@/core/services";
import type { NavItem } from "./nav-menu.types";

const PREFIX_MATCH = { exact: false };

@Component({
  selector: "app-nav-menu",
  templateUrl: "./nav-menu.html",
  styleUrl: "./nav-menu.css",
  imports: [CountBadge, Icon, NgTemplateOutlet, RouterLink, RouterLinkActive, UiTooltip],
  host: { class: "flex flex-col flex-1 min-h-0" },
})
export class NavMenu {
  private readonly router = inject(Router);
  private readonly sidebarNavService = inject(SidebarNavService);
  private readonly favoritesService = inject(SidebarFavoritesService);

  public readonly collapsed = input(false);
  public readonly itemClick = output<NavItem>();

  private readonly sections = this.sidebarNavService.menuSections;

  protected readonly expandedItemId = signal<string | null>(null);
  protected readonly hoveredItem = signal<NavItem | null>(null);
  protected readonly flyoutTop = signal(0);
  protected readonly flyoutMaxHeight = signal(400);

  private flyoutTimeout: ReturnType<typeof setTimeout> | null = null;

  /** `router.isActive` is not reactive, so group highlighting needs a navigation tick to read it. */
  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)),
  );

  protected readonly mainSections = computed(() => this.sections().filter((s) => !s.pinToBottom));

  protected readonly bottomSections = computed(() => this.sections().filter((s) => s.pinToBottom));

  private readonly allItems = computed(() => this.sections().flatMap((section) => section.items));

  /**
   * Exact only when a sibling nests under this child's route - otherwise a parent route such as
   * `/ai-dispatch` would light up on every sibling page. Everything else matches by prefix, so a
   * detail page keeps its list item marked. Derived once per nav change rather than per binding:
   * a fresh options object every tick re-runs `RouterLinkActive` on every link in the menu.
   */
  private readonly linkOptions = computed(() => {
    const options = new Map<string, { exact: boolean }>();

    for (const item of this.allItems()) {
      const children = item.children ?? [];
      for (const child of children) {
        const route = child.route;
        const exact =
          !!route && children.some((c) => c !== child && c.route?.startsWith(`${route}/`));
        options.set(child.id, exact ? { exact } : PREFIX_MATCH);
      }
    }

    return options;
  });

  protected readonly activeGroupId = computed(() => {
    this.navigationEnd();
    return this.allItems().find((item) => this.isGroupActive(item))?.id ?? null;
  });

  constructor() {
    // Deep-linking to a child route must open its group, or the active row is hidden.
    effect(() => {
      if (this.collapsed()) {
        this.expandedItemId.set(null);
        return;
      }

      const active = this.activeGroupId();
      if (active) {
        this.expandedItemId.set(active);
      }
    });
  }

  protected toggleGroup(item: NavItem): void {
    if (this.collapsed()) return;
    this.expandedItemId.set(this.expandedItemId() === item.id ? null : item.id);
  }

  /** Right-click toggles the favourites bar entry, on both the rail and the drawer. */
  protected onItemContextMenu(event: MouseEvent, item: NavItem): void {
    event.preventDefault();
    this.favoritesService.toggle(item.id);
  }

  protected isGroupExpanded(item: NavItem): boolean {
    return this.expandedItemId() === item.id;
  }

  protected childLinkOptions(child: NavItem): { exact: boolean } {
    return this.linkOptions().get(child.id) ?? PREFIX_MATCH;
  }

  private isGroupActive(item: NavItem): boolean {
    if (!item.children) return false;
    return item.children.some((child) => child.route && this.isChildActive(child));
  }

  private isChildActive(child: NavItem): boolean {
    return this.router.isActive(child.route!, {
      paths: this.childLinkOptions(child).exact ? "exact" : "subset",
      queryParams: "ignored",
      matrixParams: "ignored",
      fragment: "ignored",
    });
  }

  // -- Flyout logic for collapsed state --

  protected onItemMouseEnter(event: MouseEvent, item: NavItem): void {
    if (!this.collapsed() || !item.children) return;

    if (this.flyoutTimeout) {
      clearTimeout(this.flyoutTimeout);
      this.flyoutTimeout = null;
    }

    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const estimatedFlyoutHeight = Math.min(item.children.length * 40 + 48, 400);

    if (rect.top + estimatedFlyoutHeight > viewportHeight - 8) {
      this.flyoutTop.set(Math.max(8, viewportHeight - estimatedFlyoutHeight - 8));
    } else {
      this.flyoutTop.set(rect.top);
    }
    this.flyoutMaxHeight.set(viewportHeight - 16);
    this.hoveredItem.set(item);
  }

  protected onItemMouseLeave(): void {
    if (!this.collapsed()) return;
    this.flyoutTimeout = setTimeout(() => {
      this.hoveredItem.set(null);
    }, 150);
  }

  protected onFlyoutMouseEnter(): void {
    if (this.flyoutTimeout) {
      clearTimeout(this.flyoutTimeout);
      this.flyoutTimeout = null;
    }
  }

  protected onFlyoutMouseLeave(): void {
    this.hoveredItem.set(null);
  }

  protected onFlyoutItemClick(child: NavItem): void {
    this.hoveredItem.set(null);
    this.itemClick.emit(child);
  }

  protected onItemFocus(event: FocusEvent, item: NavItem): void {
    if (!this.collapsed() || !item.children) return;
    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    this.flyoutTop.set(rect.top);
    this.flyoutMaxHeight.set(window.innerHeight - 16);
    this.hoveredItem.set(item);
  }

  protected onItemBlur(): void {
    if (!this.collapsed()) return;
    this.flyoutTimeout = setTimeout(() => {
      this.hoveredItem.set(null);
    }, 200);
  }
}
