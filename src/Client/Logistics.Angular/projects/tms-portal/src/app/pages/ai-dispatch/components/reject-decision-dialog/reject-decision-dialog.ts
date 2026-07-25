import { Component, computed, inject, signal } from "@angular/core";
import { UiButton, UiDialog, UiTextareaField } from "@logistics/shared/ui";
import { DecisionActionsService } from "../../services/decision-actions.service";
import { buildDecisionDetail } from "../../utils/decision-utils";

/** Quick picks cover the rejection causes the agent can actually learn a preference from. */
const QUICK_REASONS = [
  "Deadhead too far",
  "Wrong driver for this customer",
  "Timing doesn't work",
  "Truck/trailer mismatch",
  "Rate too low",
] as const;

/**
 * Collects a rejection reason, then hands it to {@link DecisionActionsService}.
 *
 * The reason is the labelled training signal nightly policy learning uses, so it is required here
 * rather than optional - a bare rejection teaches the agent nothing.
 */
@Component({
  selector: "app-reject-decision-dialog",
  templateUrl: "./reject-decision-dialog.html",
  imports: [UiButton, UiDialog, UiTextareaField],
})
export class RejectDecisionDialog {
  protected readonly actions = inject(DecisionActionsService);

  protected readonly quickReasons = QUICK_REASONS;
  protected readonly reason = signal("");
  protected readonly submitAttempted = signal(false);
  protected readonly isSubmitting = signal(false);

  protected readonly detail = computed(() => {
    const decision = this.actions.pendingDecision();
    return decision ? buildDecisionDetail(decision) : "";
  });

  protected readonly isEmpty = computed(() => this.reason().trim().length === 0);
  protected readonly charsRemaining = computed(() => 500 - this.reason().length);

  protected applyQuickReason(text: string): void {
    const current = this.reason().trim();
    this.reason.set(current.length === 0 ? text : `${current}. ${text}`);
  }

  protected async confirm(): Promise<void> {
    this.submitAttempted.set(true);
    if (this.isEmpty()) return;

    this.isSubmitting.set(true);
    try {
      await this.actions.confirmReject(this.reason().trim());
      this.close();
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected close(): void {
    this.reason.set("");
    this.submitAttempted.set(false);
    this.actions.showRejectDialog.set(false);
    this.actions.pendingDecision.set(null);
  }
}
