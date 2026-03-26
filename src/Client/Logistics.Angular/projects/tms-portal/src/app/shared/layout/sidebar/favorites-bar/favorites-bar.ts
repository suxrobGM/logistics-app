import { Component, computed, inject, input, output } from "@angular/core";
import { Router } from "@angular/router";
import { PopoverModule } from "primeng/popover";
import { TooltipModule } from "primeng/tooltip";
import { SidebarFavoritesService } from "@/core/services";
import type { NavItem, NavSection } from "../../nav-menu";

@Component({
  selector: "app-favorites-bar",
  templateUrl: "./favorites-bar.html",
  imports: [TooltipModule, PopoverModule],
})
export class FavoritesBar {
  private readonly router = inject(Router);
  private readonly favoritesService = inject(SidebarFavoritesService);

  public readonly sections = input.required<NavSection[]>();
  public readonly collapsed = input(false);
  public readonly navigate = output<string>();

  protected readonly favoriteItems = computed(() => {
    const ids = this.favoritesService.favoriteIds();
    const allItems = this.flattenItems(this.sections());
    return ids
      .map((id) => allItems.find((item) => item.id === id))
      .filter((item): item is NavItem => item != null);
  });

  protected readonly availableItems = computed(() => {
    const ids = this.favoritesService.favoriteIds();
    const allItems = this.flattenItems(this.sections());
    return allItems.filter((item) => item.route && !ids.includes(item.id));
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
