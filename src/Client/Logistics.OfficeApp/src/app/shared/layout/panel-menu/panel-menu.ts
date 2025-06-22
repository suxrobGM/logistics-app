import {Component, ViewEncapsulation, effect, inject, input, model, output} from "@angular/core";
import {Router} from "@angular/router";
import {PanelMenuModule} from "primeng/panelmenu";
import {TooltipModule} from "primeng/tooltip";
import {MenuItem} from "./types";

@Component({
  selector: "app-panel-menu",
  templateUrl: "./panel-menu.html",
  styleUrl: "./panel-menu.css",
  imports: [PanelMenuModule, TooltipModule],
  encapsulation: ViewEncapsulation.None,
})
export class PanelMenu {
  private readonly router = inject(Router);
  public readonly items = input.required<MenuItem[]>();
  public readonly expanded = model<boolean>(false);
  public readonly itemClick = output<MenuItem>();
  public readonly styleClass = input<string>();

  constructor() {
    effect(() => {
      // Collapse all nested menus when the main menu is collapsed
      if (!this.expanded()) {
        this.collapseAllItems();
      }
    });
  }

  protected toggleMenuItem(ev: MouseEvent, item: MenuItem): void {
    // Prevent default behavior if the item has children and expand the submenu
    if (!this.expanded() && item.items) {
      this.expanded.set(true);
      ev.stopPropagation();
    } else {
      this.naviageTo(item.route);

      if (this.expanded()) {
        this.itemClick.emit(item);
      }
    }
  }

  private collapseAllItems(): void {
    // Force collapse all submenus by resetting expanded state
    this.items().forEach((i) => (i.expanded = false));
  }

  private naviageTo(route: string | undefined): void {
    if (route) {
      this.router.navigateByUrl(route);
    }
  }
}
