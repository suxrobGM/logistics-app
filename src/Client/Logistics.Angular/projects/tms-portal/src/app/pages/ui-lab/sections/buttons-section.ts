import { Component, signal } from "@angular/core";
import {
  Typography,
  UiButton,
  type UiButtonAppearance,
  type UiButtonIntent,
  type UiButtonSize,
} from "@logistics/shared/ui";

/**
 * `ui-button`, every cell.
 *
 * The S4 component step rendered a `<p-button>` beside each of these so a human could see which of
 * the 32 intent x appearance cells had moved. The S4 SWEEP then took all 498 call sites, so there is
 * no PrimeNG button left in the workspace: the comparison column is gone, and with it the
 * `LabAppearance` / `LabSize` shapes that existed only to translate our vocabulary into PrimeNG's
 * (`outlined`/`text`/`link` booleans, `small`/`large`). The matrix now iterates the real unions, so
 * adding an intent or an appearance to the type makes it appear here with no edit to this file.
 *
 * What survives is the part that is still an open question: the TMS primary gradient. The template
 * poses it with two `ui-button`s and a scoped `--ui-btn-primary-*` reset instead of a p-button.
 */
@Component({
  selector: "app-ui-lab-buttons",
  templateUrl: "./buttons-section.html",
  imports: [Typography, UiButton],
})
export class UiLabButtonsSection {
  protected readonly intents: readonly UiButtonIntent[] = [
    "primary",
    "secondary",
    "success",
    "info",
    "warn",
    "danger",
    "help",
    "contrast",
  ];

  protected readonly appearances: readonly UiButtonAppearance[] = [
    "solid",
    "outlined",
    "text",
    "link",
  ];

  protected readonly sizes: readonly UiButtonSize[] = ["sm", "md", "lg"];

  /** Drives the loading demo, so the spinner can be watched starting and stopping, not just staring. */
  protected readonly busy = signal(false);

  protected toggleBusy(): void {
    this.busy.set(true);
    setTimeout(() => this.busy.set(false), 1600);
  }
}
