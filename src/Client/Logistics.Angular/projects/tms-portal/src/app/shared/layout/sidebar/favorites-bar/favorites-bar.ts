import { Component, computed, inject, input, output } from "@angular/core";
import { Router } from "@angular/router";
import { Icon, UiPopover, UiTooltip } from "@logistics/shared/ui";
import { SidebarFavoritesService, SidebarNavService } from "@/core/services";
import type { NavItem, NavSection } from "../../nav-menu";

@Component({
  selector: "app-favorites-bar",
  templateUrl: "./favorites-bar.html",
  imports: [Icon, UiPopover, UiTooltip],
})
export class FavoritesBar {
  private readonly router = inject(Router);
  private readonly favoritesService = inject(SidebarFavoritesService);
  private readonly sidebarNavService = inject(SidebarNavService);

  public readonly collapsed = input(false);
  public readonly navigate = output<string>();

  /** The full tree, hidden children included - a favorite may point at a `menuHidden` child. */
  private readonly allItems = computed(() =>
    this.flattenItems(this.sidebarNavService.fullSections()),
  );

  protected readonly favoriteItems = computed(() => {
    const allItems = this.allItems();
    return this.favoritesService
      .favoriteIds()
      .map((id) => allItems.find((item) => item.id === id))
      .filter((item): item is NavItem => item != null);
  });

  protected readonly availableItems = computed(() => {
    const ids = this.favoritesService.favoriteIds();
    return this.allItems().filter((item) => item.route && !ids.includes(item.id));
  });

  protected readonly isFull = computed(() => this.favoritesService.isFull());

  protected onFavoriteClick(item: NavItem): void {
    if (item.route) {
      this.router.navigateByUrl(item.route);
      this.navigate.emit(item.route);
    }
  }

  protected removeFavorite(event: MouseEvent, itemId: string): void {
    event.stopPropagation();
    this.favoritesService.remove(itemId);
  }

  protected addFavorite(itemId: string): void {
    this.favoritesService.add(itemId);
  }

  private flattenItems(sections: NavSection[]): NavItem[] {
    const items: NavItem[] = [];
    for (const section of sections) {
      for (const item of section.items) {
        items.push(item);
        if (item.children) {
          items.push(...item.children);
        }
      }
    }
    return items;
  }
}
