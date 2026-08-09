import { Component, input } from "@angular/core";
import { Spinner, Stack, Typography } from "@logistics/shared/ui";

/** The "still working" line under an agent transcript while a turn runs. */
@Component({
  selector: "app-turn-status",
  templateUrl: "./turn-status.html",
  imports: [Spinner, Stack, Typography],
  host: { class: "block" },
})
export class TurnStatus {
  /** What the agent is called while it has not reported a tool yet. */
  public readonly workingLabel = input.required<string>();
  /** Tools run so far in this turn; null until the first per-iteration update lands. */
  public readonly toolsRun = input<number | null>(null);
  public readonly longRunning = input(false);
}
