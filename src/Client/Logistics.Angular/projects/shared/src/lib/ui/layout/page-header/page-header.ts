import { Component, input } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";

/**
 * Page header component with title, optional subtitle, and optional add button.
 */
@Component({
  selector: "ui-page-header",
  templateUrl: "./page-header.html",
  imports: [ButtonModule, RouterModule, TooltipModule],
})
export class PageHeader {
  public readonly title = input.required<string>();
  public readonly subtitle = input<string | null>(null);
  public readonly addRoute = input<string | null>(null);
  public readonly addTooltip = input<string>("Add new item");
  public readonly centered = input<boolean>(false);
  public readonly backLink = input<string | null>(null);
}
