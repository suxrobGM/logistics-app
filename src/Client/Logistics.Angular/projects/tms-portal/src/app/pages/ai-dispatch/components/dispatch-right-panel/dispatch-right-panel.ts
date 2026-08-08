import { Component, inject } from "@angular/core";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { Badge, EmptyState, Icon, Stack, Surface, Typography } from "@logistics/shared/ui";
import { AIQuotaUsage, DecisionActionsService, GeolocationMap } from "@/shared/components";
import { DispatchChatStore } from "../../store/dispatch-chat.store";
import { DecisionCard } from "../decision-card/decision-card";

/**
 * Right panel of the dispatch chat page: fleet map, tenant-wide pending write decisions, and the
 * AI quota meter. Collapsible - the page header owns the toggle button, the store owns the state.
 */
@Component({
  selector: "app-dispatch-right-panel",
  templateUrl: "./dispatch-right-panel.html",
  imports: [
    AIQuotaUsage,
    Badge,
    DecisionCard,
    EmptyState,
    GeolocationMap,
    Icon,
    Stack,
    Surface,
    Typography,
  ],
})
export class DispatchRightPanel {
  protected readonly store = inject(DispatchChatStore);
  protected readonly actions = inject(DecisionActionsService);

  protected approve(decision: AgentDecisionDto): void {
    this.actions.approve(decision, () => this.store.onDecisionResolved());
  }

  protected reject(decision: AgentDecisionDto): void {
    this.actions.reject(decision, () => this.store.onDecisionResolved());
  }
}
