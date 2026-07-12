import { Component, computed, input, model, output } from "@angular/core";
import type { AiDispatchMode, AiQuotaStatusDto } from "@logistics/shared/api";
import {
  Alert,
  UiButton,
  UiDialog,
  UiTextareaField,
  type IconName,
  type UiButtonIntent,
} from "@logistics/shared/ui";

export interface RunAgentDialogData {
  mode: AiDispatchMode;
  instructions?: string;
}

@Component({
  selector: "app-run-agent-dialog",
  templateUrl: "./run-agent-dialog.html",
  imports: [Alert, UiButton, UiDialog, UiTextareaField],
})
export class RunAgentDialog {
  public readonly visible = model(false);
  public readonly mode = input<AiDispatchMode>("human_in_the_loop");
  public readonly quotaStatus = input<AiQuotaStatusDto | null>(null);
  public readonly run = output<RunAgentDialogData>();

  protected readonly instructions = model("");

  protected readonly isOverQuota = computed(() => this.quotaStatus()?.isOverQuota === true);
  protected readonly charsRemaining = computed(() => 500 - this.instructions().length);

  protected get modeLabel(): string {
    return this.mode() === "human_in_the_loop" ? "Run (Suggestions)" : "Run (Autonomous)";
  }

  protected get modeIcon(): IconName {
    return this.mode() === "human_in_the_loop" ? "play" : "zap";
  }

  /** Autonomous mode moves loads without asking, so its Run button is deliberately a warning. */
  protected get modeIntent(): UiButtonIntent {
    return this.mode() === "human_in_the_loop" ? "primary" : "warn";
  }

  protected confirm(): void {
    this.run.emit({
      mode: this.mode(),
      instructions: this.instructions().trim(),
    });
    this.instructions.set("");
    this.visible.set(false);
  }

  protected cancel(): void {
    this.instructions.set("");
    this.visible.set(false);
  }
}
