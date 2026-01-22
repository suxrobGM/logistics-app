import { Component, input, output } from "@angular/core";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-filter-tabs",
  templateUrl: "./filter-tabs.html",
  imports: [ScrollAnimateDirective],
})
export class FilterTabs {
  public readonly options = input.required<string[]>();
  public readonly selected = input.required<string>();
  public readonly variant = input<"light" | "dark">("light");
  public readonly delay = input(100);

  public readonly selectionChange = output<string>();
}
