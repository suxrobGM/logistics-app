import { Component, input, output } from "@angular/core";
import type { AgentDecisionDto, AIQuotaStatusDto } from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { Badge, EmptyState, Icon, Stack, Surface, Typography } from "@logistics/shared/ui";
import { AIQuotaUsage, GeolocationMap } from "@/shared/components";
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
  public readonly truckLocations = input.required<TruckGeolocationDto[]>();
  public readonly pendingDecisions = input.required<AgentDecisionDto[]>();
  public readonly quota = input<AIQuotaStatusDto | null>(null);
  public readonly busyDecisionId = input<string | null>(null);

  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();
}
