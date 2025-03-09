import {Component, effect, input, model, output} from "@angular/core";
import {PanelMenuModule} from "primeng/panelmenu";
import {MenuItem} from "./types";
import {Router} from "@angular/router";

@Component({
  selector: "app-panel-menu",
  imports: [PanelMenuModule],
  templateUrl: "./panel-menu.component.html",
  styleUrl: "./panel-menu.component.scss",
})
export class PanelMenuComponent {
  readonly items = input.required<MenuItem[]>();
  readonly expanded = model<boolean>(false);
  readonly itemClick = output<MenuItem>();
  readonly styleClass = input<string>();

  constructor(private readonly router: Router) {
    effect(() => {
      // Collapse all nested menus when the main menu is collapsed
      if (!this.expanded()) {
        this.collapseAllItems();
      }
    });
  }

  handleClick(ev: MouseEvent, item: MenuItem): void {
    if (!this.expanded()) {
      this.expanded.set(true);
      ev.stopPropagation();
      return;
    }

    if (item.route) {
      this.router.navigateByUrl(item.route);
    }

    this.itemClick.emit(item);
  }

  private collapseAllItems(): void {
    // Force collapse all submenus by resetting expanded state
    this.items().forEach((i) => (i.expanded = false));
  }
}
