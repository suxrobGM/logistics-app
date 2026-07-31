import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { AIQuotaStatusDto } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Badge, Icon, Progress } from "@logistics/shared/ui";

@Component({
  selector: "app-ai-quota-usage",
  templateUrl: "./ai-quota-usage.html",
  imports: [Badge, CurrencyFormatPipe, DatePipe, Icon, Progress],
})
export class AIQuotaUsage {
  readonly quota = input.required<AIQuotaStatusDto>();

  protected readonly Math = Math;

  protected readonly progressBarColor = computed(() => {
    const pct = (this.quota().usagePercent ?? 0) * 100;
    if (pct >= 90) return "var(--danger)";
    if (pct >= 70) return "var(--warning)";
    return "";
  });
}
