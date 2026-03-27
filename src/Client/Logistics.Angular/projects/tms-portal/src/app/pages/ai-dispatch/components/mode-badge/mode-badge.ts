import { Component, input } from "@angular/core";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-mode-badge",
  templateUrl: "./mode-badge.html",
  imports: [TagModule],
})
export class ModeBadge {
  public readonly mode = input.required<string | null | undefined>();
}
