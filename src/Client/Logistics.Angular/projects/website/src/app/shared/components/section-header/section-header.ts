import { Component, input } from "@angular/core";

@Component({
  selector: "web-section-header",
  templateUrl: "./section-header.html",
})
export class SectionHeader {
  public readonly label = input.required<string>();
  public readonly title = input.required<string>();
  public readonly subtitle = input<string>();
  public readonly dark = input(false);
}
