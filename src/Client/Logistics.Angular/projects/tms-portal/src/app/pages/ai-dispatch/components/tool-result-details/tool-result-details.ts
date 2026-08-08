import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject, input } from "@angular/core";
import { LocalizationService } from "@logistics/shared";
import type { AgentToolResultDto } from "@logistics/shared/api";
import { UiDataTable } from "@logistics/shared/ui";

/** Compact table rendering of a tool result's trucks or loads, for inline display in chat turns. */
@Component({
  selector: "app-tool-result-details",
  templateUrl: "./tool-result-details.html",
  imports: [CurrencyPipe, UiDataTable],
  host: { "[class.hidden]": "!hasContent()" },
})
export class ToolResultDetails {
  public readonly result = input<AgentToolResultDto | null | undefined>();

  protected readonly localization = inject(LocalizationService);

  protected readonly trucks = computed(() => this.result()?.trucks ?? null);
  protected readonly loads = computed(() => this.result()?.loads ?? null);

  protected readonly hasContent = computed(
    () => (this.trucks()?.length ?? 0) > 0 || (this.loads()?.length ?? 0) > 0,
  );
}
