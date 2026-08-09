import { Component, output } from "@angular/core";
import { UiButton } from "@logistics/shared/ui";

const SUGGESTED_PROMPTS = [
  "Which loads were delivered last week?",
  "Show unpaid invoices",
  "Any trucks free tomorrow?",
  "What did we spend on fuel this month?",
] as const;

/** Empty-transcript state for the copilot drawer, mirroring `app-dispatch-welcome` on the page. */
@Component({
  selector: "app-copilot-welcome",
  templateUrl: "./copilot-welcome.html",
  host: { class: "block" },
  imports: [UiButton],
})
export class CopilotWelcome {
  public readonly promptSelected = output<string>();

  protected readonly suggestedPrompts = SUGGESTED_PROMPTS;
}
