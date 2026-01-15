import { Component, input } from "@angular/core";

/**
 * Page title component with optional subtitle.
 */
@Component({
  selector: "app-page-title",
  templateUrl: "./page-title.html",
})
export class PageTitle {
  public readonly title = input.required<string>();
  public readonly subtitle = input<string | null>(null);
}
