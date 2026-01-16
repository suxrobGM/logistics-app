import { Component, input, output } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";

/**
 * Page header component with title, optional subtitle, and optional add button.
 */
@Component({
  selector: "app-page-header",
  templateUrl: "./page-header.html",
  imports: [ButtonModule, RouterModule, TooltipModule],
})
export class PageHeader {
  public readonly title = input.required<string>();
  public readonly subtitle = input<string | null>(null);
  public readonly addRoute = input<string | null>(null);
  public readonly addTooltip = input<string>("Add new item");
  public readonly showAddButton = input<boolean>(true);
  public readonly addClick = output<void>();

  protected handleAdd(): void {
    this.addClick.emit();
  }
}
