import { Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { DispatchAgentMode } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { TextareaModule } from "primeng/textarea";

export interface RunAgentDialogData {
  mode: DispatchAgentMode;
  instructions?: string;
}

@Component({
  selector: "app-run-agent-dialog",
  templateUrl: "./run-agent-dialog.html",
  imports: [DialogModule, ButtonModule, FormsModule, TextareaModule],
})
export class RunAgentDialog {
  public readonly visible = model(false);
  public readonly mode = input<DispatchAgentMode>("human_in_the_loop");
  public readonly run = output<RunAgentDialogData>();

  protected readonly instructions = model("");

  protected get modeLabel(): string {
    return this.mode() === "human_in_the_loop" ? "Run (Suggestions)" : "Run (Autonomous)";
  }

  protected get modeIcon(): string {
    return this.mode() === "human_in_the_loop" ? "pi pi-play" : "pi pi-bolt";
  }

  protected get modeSeverity(): "primary" | "warn" {
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
