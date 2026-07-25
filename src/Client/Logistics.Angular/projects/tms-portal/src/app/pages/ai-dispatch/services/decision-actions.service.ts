import { inject, Injectable, signal } from "@angular/core";
import {
  Api,
  approveAiDispatchDecision,
  rejectAiDispatchDecision,
  type AiDispatchDecisionDto,
} from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { buildDecisionDetail } from "../utils/decision-utils";

/**
 * Approve/reject for agent decisions, shared by the sessions list and the session details page.
 *
 * Owns the reject dialog's state and the wire calls so the required-reason rule and the request shape
 * exist once - the reason is the labelled signal nightly policy learning depends on, and a second
 * copy is a second place to forget it.
 *
 * Provided per page (`providers: [DecisionActionsService]`), not app-wide.
 */
@Injectable()
export class DecisionActionsService {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly showRejectDialog = signal(false);
  public readonly pendingDecision = signal<AiDispatchDecisionDto | null>(null);

  private onComplete?: () => void | Promise<void>;

  approve(decision: AiDispatchDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;

    this.toast.confirm({
      message: `Are you sure you want to approve and execute this decision?\n\n${buildDecisionDetail(decision)}`,
      header: "Approve Decision",
      icon: "success",
      severity: "success",
      accept: async () => {
        try {
          await this.api.invoke(approveAiDispatchDecision, { decisionId: decision.id! });
          this.toast.showSuccess("Decision approved and executed");
          await this.onComplete?.();
        } catch {
          this.toast.showError("Failed to approve decision");
        }
      },
    });
  }

  /** Opens the reason dialog; the rejection itself happens in {@link confirmReject}. */
  reject(decision: AiDispatchDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;
    this.pendingDecision.set(decision);
    this.showRejectDialog.set(true);
  }

  async confirmReject(reason: string): Promise<void> {
    const decision = this.pendingDecision();
    if (!decision) return;

    try {
      await this.api.invoke(rejectAiDispatchDecision, {
        decisionId: decision.id!,
        body: { reason },
      });
      this.toast.showSuccess("Decision rejected");
      await this.onComplete?.();
    } catch {
      this.toast.showError("Failed to reject decision");
    }
  }
}
