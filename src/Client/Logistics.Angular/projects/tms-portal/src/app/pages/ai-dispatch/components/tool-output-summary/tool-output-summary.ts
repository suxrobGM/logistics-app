import { Component, computed, input } from "@angular/core";
import { type ParsedToolOutput, parseToolOutput } from "../../utils/decision-utils";

@Component({
  selector: "app-tool-output-summary",
  templateUrl: "./tool-output-summary.html",
})
export class ToolOutputSummary {
  public readonly toolOutput = input.required<string | null | undefined>();

  protected readonly output = computed<ParsedToolOutput>(() => parseToolOutput(this.toolOutput()));

  protected readonly allBatchFailed = computed(() => {
    const results = this.output().batchResults;
    return results ? results.length > 0 && results.every((r) => !r.feasible) : false;
  });
}
