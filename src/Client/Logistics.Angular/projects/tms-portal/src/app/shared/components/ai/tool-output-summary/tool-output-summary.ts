import { Component, computed, input } from "@angular/core";
import type { AgentToolResultDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";

@Component({
  selector: "app-tool-output-summary",
  templateUrl: "./tool-output-summary.html",
  imports: [Icon],
})
export class ToolOutputSummary {
  public readonly result = input.required<AgentToolResultDto | null | undefined>();

  protected readonly output = computed<AgentToolResultDto>(() => this.result() ?? {});

  protected readonly hasVerdict = computed(() => typeof this.output().feasible === "boolean");
  protected readonly hasOutcome = computed(() => typeof this.output().success === "boolean");

  protected readonly allBatchFailed = computed(() => {
    const results = this.output().results;
    return results ? results.length > 0 && results.every((r) => !r.feasible) : false;
  });
}
