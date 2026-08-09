import { inject, Injectable, signal } from "@angular/core";
import {
  Api,
  approveAIDispatchDecision,
  approveCopilotDecision,
  rejectAIDispatchDecision,
  rejectCopilotDecision,
  type AgentDecisionDto,
} from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { buildDecisionDetail } from "@/shared/utils";

/** Which decision endpoints the consumer's suggestions live behind. */
type DecisionEndpoint = "dispatch" | "copilot";

/**
 * Approve/reject for agent decisions, shared by the dispatch pages and the copilot drawer, so the
 * required-reason rule exists once. Provided per consumer, not app-wide: the copilot drawer calls
 * `configure("copilot")`, dispatch pages keep the default.
 */
@Injectable()
export class DecisionActionsService {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly showRejectDialog = signal(false);
  public readonly pendingDecision = signal<AgentDecisionDto | null>(null);
  /** Id of the decision whose approve/reject request is in flight, for per-card busy state. */
  public readonly busyDecisionId = signal<string | null>(null);

  private endpoint: DecisionEndpoint = "dispatch";
  private onComplete?: () => void | Promise<void>;

  configure(endpoint: DecisionEndpoint): void {
    this.endpoint = endpoint;
  }

  approve(decision: AgentDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;

    this.toast.confirm({
      message: `Are you sure you want to approve and execute this decision?\n\n${buildDecisionDetail(decision)}`,
      header: "Approve Decision",
      icon: "success",
      severity: "success",
      accept: () =>
        this.run(
          decision,
          () =>
            this.api.invoke(
              this.endpoint === "copilot" ? approveCopilotDecision : approveAIDispatchDecision,
              { decisionId: decision.id! },
            ),
          "Decision approved and executed",
        ),
    });
  }

  /** Opens the reason dialog; the rejection itself happens in {@link confirmReject}. */
  reject(decision: AgentDecisionDto, onComplete?: () => void | Promise<void>): void {
    this.onComplete = onComplete;
    this.pendingDecision.set(decision);
    this.showRejectDialog.set(true);
  }

  async confirmReject(reason: string): Promise<void> {
    const decision = this.pendingDecision();
    if (!decision) return;

    await this.run(
      decision,
      () =>
        this.api.invoke(
          this.endpoint === "copilot" ? rejectCopilotDecision : rejectAIDispatchDecision,
          { decisionId: decision.id!, body: { reason } },
        ),
      "Decision rejected",
    );
  }

  /**
   * Owns the in-flight bookkeeping both verbs share. The failure toast comes from the global
   * errorHandlerInterceptor, so this only reports success and clears the busy marker.
   */
  private async run(
    decision: AgentDecisionDto,
    operation: () => Promise<unknown>,
    successMessage: string,
  ): Promise<void> {
    this.busyDecisionId.set(decision.id ?? null);
    try {
      await operation();
      this.toast.showSuccess(successMessage);
      await this.onComplete?.();
    } catch {
      // Already surfaced by the global error interceptor.
    } finally {
      this.busyDecisionId.set(null);
    }
  }
}
