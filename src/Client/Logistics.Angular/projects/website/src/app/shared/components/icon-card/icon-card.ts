import { Component, input } from "@angular/core";
import { IconCircle } from "../icon-circle/icon-circle";
import { ScrollAnimateDirective } from "@/shared/directives";

export interface IconCardItem {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: "web-icon-card",
  templateUrl: "./icon-card.html",
  imports: [IconCircle, ScrollAnimateDirective],
})
export class IconCard {
  public readonly icon = input.required<string>();
  public readonly title = input.required<string>();
  public readonly description = input.required<string>();
  public readonly dark = input(false);
  public readonly delay = input(0);
  public readonly variant = input<"accent" | "ink" | "outlined">("accent");
}
