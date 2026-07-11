import { Component, input } from "@angular/core";
import { Badge } from "@logistics/shared/ui";

@Component({
  selector: "app-mode-badge",
  templateUrl: "./mode-badge.html",
  imports: [Badge],
})
export class ModeBadge {
  public readonly mode = input.required<string | null | undefined>();
}
