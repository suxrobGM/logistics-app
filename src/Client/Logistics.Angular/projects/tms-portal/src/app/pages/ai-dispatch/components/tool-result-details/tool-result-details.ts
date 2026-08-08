import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject, input } from "@angular/core";
import { LocalizationService } from "@logistics/shared";
import { UiDataTable } from "@logistics/shared/ui";
import { parseToolOutput, type ParsedToolOutput } from "@/shared/utils";

type ParsedTruck = NonNullable<ParsedToolOutput["trucks"]>[number];
type ParsedLoad = NonNullable<ParsedToolOutput["loads"]>[number];

/** Compact table rendering of a tool result's trucks or loads, for inline display in chat turns. */
@Component({
  selector: "app-tool-result-details",
  templateUrl: "./tool-result-details.html",
  imports: [CurrencyPipe, UiDataTable],
  host: { "[class.hidden]": "!hasContent()" },
})
export class ToolResultDetails {
  public readonly toolOutput = input<string | null | undefined>();

  protected readonly localization = inject(LocalizationService);

  private readonly output = computed<ParsedToolOutput>(() => parseToolOutput(this.toolOutput()));

  protected readonly trucks = computed<ParsedTruck[] | null>(() => this.output().trucks ?? null);
  protected readonly loads = computed<ParsedLoad[] | null>(() => this.output().loads ?? null);

  protected readonly hasContent = computed(
    () => (this.trucks()?.length ?? 0) > 0 || (this.loads()?.length ?? 0) > 0,
  );
}
