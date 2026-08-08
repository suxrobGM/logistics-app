import { Component, output } from "@angular/core";
import { Icon, Surface, Typography } from "@logistics/shared/ui";
import { QUICK_ACTIONS } from "./quick-actions";

/** Empty-transcript state: what the dispatcher agent does, plus one-click starting prompts. */
@Component({
  selector: "app-dispatch-welcome",
  templateUrl: "./dispatch-welcome.html",
  imports: [Icon, Surface, Typography],
  host: { class: "flex min-h-full flex-col justify-center gap-6 py-4" },
})
export class DispatchWelcome {
  public readonly action = output<string>();

  protected readonly quickActions = QUICK_ACTIONS;
}
