import { Component, inject, output } from "@angular/core";
import { Icon, UiTooltip } from "@logistics/shared/ui";
import { CopilotStore } from "@/core/store";

/**
 * Opens the copilot drawer, or the upsell when the plan doesn't include it. Renders nothing when
 * the tenant cannot see the feature at all. Sits beside `app-notification-bell` and
 * `app-theme-toggle`; the footer box comes from `.nav-footer-actions` in the portal chrome.
 */
@Component({
  selector: "app-copilot-launcher",
  templateUrl: "./copilot-launcher.html",
  styleUrl: "./copilot-launcher.css",
  imports: [Icon, UiTooltip],
})
export class CopilotLauncher {
  protected readonly store = inject(CopilotStore);

  /** The mobile drawer closes itself before the copilot takes over the screen. */
  public readonly triggered = output<void>();

  protected open(): void {
    this.triggered.emit();
    void this.store.openOrUpsell();
  }
}
