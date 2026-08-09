import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { AIQuotaStatusDto } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Badge, Icon, Progress, Surface } from "@logistics/shared/ui";

@Component({
  selector: "app-ai-quota-usage",
  templateUrl: "./ai-quota-usage.html",
  imports: [Badge, CurrencyFormatPipe, DatePipe, Icon, Progress, Surface],
})
export class AIQuotaUsage {
  readonly quota = input.required<AIQuotaStatusDto>();

  protected readonly usagePercentRounded = computed(() =>
    Math.round(Math.min((this.quota().usagePercent ?? 0) * 100, 100)),
  );

  protected readonly progressBarColor = computed(() => {
    const pct = (this.quota().usagePercent ?? 0) * 100;
    if (pct >= 90) return "var(--danger)";
    if (pct >= 70) return "var(--warning)";
    return "";
  });
}
