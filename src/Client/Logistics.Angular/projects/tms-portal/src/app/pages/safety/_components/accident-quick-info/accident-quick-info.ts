import { Component, input } from "@angular/core";
import type { AccidentReportDto } from "@logistics/shared/api";
import { Card } from "@logistics/shared/ui";

@Component({
  selector: "app-accident-quick-info",
  templateUrl: "./accident-quick-info.html",
  imports: [Card],
})
export class AccidentQuickInfo {
  public readonly report = input.required<AccidentReportDto>();
}
