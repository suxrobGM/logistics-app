import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { BadgeModule } from "primeng/badge";
import { TooltipModule } from "primeng/tooltip";
import type { NavItem, NavSection } from "./nav-menu.types";

@Component({
  selector: "app-nav-menu",
  templateUrl: "./nav-menu.html",
  styleUrl: "./nav-menu.css",
  imports: [RouterLink, RouterLinkActive, BadgeModule, TooltipModule],
  host: { class: "flex flex-col flex-1 min-h-0" },
})
export class NavMenu {
  private readonly router = inject(Router);

  public readonly sections = input.required<NavSection[]>();
  public readonly collapsed = input(false);
  public readonly itemClick = output<NavItem>();
  public readonly itemContextMenu = output<{ event: MouseEvent; item: NavItem }>();

  protected readonly expandedItemId = signal<string | null>(null);
  protected readonly hoveredItem = signal<NavItem | null>(null);
  protected readonly flyoutTop = signal(0);
  protected readonly flyoutMaxHeight = signal(400);

  private flyoutTimeout: ReturnType<typeof setTimeout> | null = null;

  protected readonly mainSections = computed(() => this.sections().filter((s) => !s.pinToBottom));

  protected readonly bottomSections = computed(() => this.sections().filter((s) => s.pinToBottom));

  constructor() {
    effect(() => {
      if (this.collapsed()) {
        this.expandedItemId.set(null);
      }
    });
  }

  protected toggleGroup(item: NavItem): void {
    if (this.collapsed()) return;
    this.expandedItemId.set(this.expandedItemId() === item.id ? null : item.id);
  }

  protected navigateTo(item: NavItem): void {
    if (item.route) {
      this.router.navigateByUrl(item.route);
      this.itemClick.emit(item);
    }
  }

  protected onItemContextMenu(event: MouseEvent, item: NavItem): void {
    event.preventDefault();
    this.itemContextMenu.emit({ event, item });
  }

  protected onChildContextMenu(event: MouseEvent, child: NavItem): void {
    event.preventDefault();
    this.itemContextMenu.emit({ event, item: child });
  }

  protected isGroupExpanded(item: NavItem): boolean {
    return this.expandedItemId() === item.id;
  }

  protected isItemActive(item: NavItem): boolean {
    if (item.route) {
      return this.router.isActive(item.route, {
        paths: "exact",
        queryParams: "ignored",
        matrixParams: "ignored",
        fragment: "ignored",
      });
    }
    return false;
  }

  protected isGroupActive(item: NavItem): boolean {
    if (!item.children) return false;
    return item.children.some((child) => child.route && this.isItemActive(child));
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
    this.navigateTo(child);
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
