import { Component, ElementRef, computed, effect, inject, signal, viewChild } from "@angular/core";
import { Router } from "@angular/router";
import { CommandPaletteService, type SearchableItem } from "@/core/services";

@Component({
  selector: "app-command-palette",
  templateUrl: "./command-palette.html",
  styleUrl: "./command-palette.css",
  host: {
    "(document:keydown)": "onKeydown($event)",
  },
})
export class CommandPalette {
  private readonly router = inject(Router);
  private readonly paletteService = inject(CommandPaletteService);
  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>("searchInput");

  protected readonly isOpen = this.paletteService.isOpen;
  protected readonly query = signal("");
  protected readonly selectedIndex = signal(0);

  protected readonly results = computed(() => this.paletteService.search(this.query()));

  constructor() {
    effect(() => {
      if (this.isOpen()) {
        setTimeout(() => this.searchInput()?.nativeElement.focus(), 0);
        this.query.set("");
        this.selectedIndex.set(0);
      }
    });
  }

  protected onKeydown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key === "k") {
      event.preventDefault();
      this.paletteService.toggle();
      return;
    }

    if (!this.isOpen()) return;

    switch (event.key) {
      case "Escape":
        this.paletteService.close();
        break;
      case "ArrowDown":
        event.preventDefault();
        this.selectedIndex.update((i) => Math.min(i + 1, this.results().length - 1));
        break;
      case "ArrowUp":
        event.preventDefault();
        this.selectedIndex.update((i) => Math.max(i - 1, 0));
        break;
      case "Enter":
        event.preventDefault();
        this.selectItem(this.results()[this.selectedIndex()]);
        break;
    }
  }

  protected onQueryChange(value: string): void {
    this.query.set(value);
    this.selectedIndex.set(0);
  }

  protected selectItem(item: SearchableItem | undefined): void {
    if (!item?.route) return;
    this.router.navigateByUrl(item.route);
    this.paletteService.close();
  }

  protected close(): void {
    this.paletteService.close();
  }

  protected onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains("palette-backdrop")) {
      this.paletteService.close();
    }
  }
}
