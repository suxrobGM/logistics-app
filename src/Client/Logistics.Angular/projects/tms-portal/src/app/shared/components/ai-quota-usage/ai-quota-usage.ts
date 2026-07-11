import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { AiQuotaStatusDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";
import { ProgressBar } from "primeng/progressbar";
import { TagModule } from "primeng/tag";

@Component({
  selector: "app-ai-quota-usage",
  templateUrl: "./ai-quota-usage.html",
  imports: [DatePipe, Icon, ProgressBar, TagModule],
})
export class AiQuotaUsage {
  readonly quota = input.required<AiQuotaStatusDto>();

  protected readonly Math = Math;

  protected readonly progressBarColor = computed(() => {
    const pct = (this.quota().usagePercent ?? 0) * 100;
    if (pct >= 90) return "var(--red-500)";
    if (pct >= 70) return "var(--yellow-500)";
    return "";
  });
}
