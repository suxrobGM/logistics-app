import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { AiQuotaStatusDto } from "@logistics/shared/api";
import { Badge, Icon, Progress } from "@logistics/shared/ui";

@Component({
  selector: "app-ai-quota-usage",
  templateUrl: "./ai-quota-usage.html",
  imports: [Badge, DatePipe, Icon, Progress],
})
export class AiQuotaUsage {
  readonly quota = input.required<AiQuotaStatusDto>();

  protected readonly Math = Math;

  protected readonly progressBarColor = computed(() => {
    const pct = (this.quota().usagePercent ?? 0) * 100;
    if (pct >= 90) return "var(--danger)";
    if (pct >= 70) return "var(--warning)";
    return "";
  });
}
