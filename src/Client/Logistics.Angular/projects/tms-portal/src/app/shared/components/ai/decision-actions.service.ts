import { inject, Injectable, signal } from "@angular/core";
import {
  Api,
  approveAIDispatchDecision,
  approveCopilotDecision,
  rejectAIDispatchDecision,
  rejectCopilotDecision,
  type AIDispatchDecisionDto,
} from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { buildDecisionDetail } from "@/shared/utils";

/** Which decision endpoints the consumer's suggestions live behind. */
export type DecisionEndpoint = "dispatch" | "copilot";

/**
 * Approve/reject for agent decisions, shared by the dispatch pages and the copilot drawer.
 * Owns the reject dialog state and the wire calls so the required-reason rule exists once.
 *
 * Provided per consumer (`providers: [DecisionActionsService]`), not app-wide. The copilot drawer
 * calls `configure("copilot")`; dispatch pages keep the default.
 */
@Injectable()
export class DecisionActionsService {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly showRejectDialog = signal(false);
  public readonly pendingDecision = signal<AIDispatchDecisionDto | null>(null);

  private endpoint: DecisionEndpoint = "dispatch";
  private onComplete?: () => void | Promise<void>;

  configure(endpoint: DecisionEndpoint): void {
    this.endpoint = endpoint;
  }

  approve(decision: AIDispatchDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;

    this.toast.confirm({
      message: `Are you sure you want to approve and execute this decision?\n\n${buildDecisionDetail(decision)}`,
      header: "Approve Decision",
      icon: "success",
      severity: "success",
      accept: async () => {
        try {
          const operation =
            this.endpoint === "copilot" ? approveCopilotDecision : approveAIDispatchDecision;
          await this.api.invoke(operation, { decisionId: decision.id! });
          this.toast.showSuccess("Decision approved and executed");
          await this.onComplete?.();
        } catch {
          this.toast.showError("Failed to approve decision");
        }
      },
    });
  }

  /** Opens the reason dialog; the rejection itself happens in {@link confirmReject}. */
  reject(decision: AIDispatchDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;
    this.pendingDecision.set(decision);
    this.showRejectDialog.set(true);
  }

  async confirmReject(reason: string): Promise<void> {
    const decision = this.pendingDecision();
    if (!decision) return;

    try {
      const operation =
        this.endpoint === "copilot" ? rejectCopilotDecision : rejectAIDispatchDecision;
      await this.api.invoke(operation, {
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
