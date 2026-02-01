import { Component, input } from "@angular/core";
import type { AccidentReportDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-accident-quick-info",
  templateUrl: "./accident-quick-info.html",
  imports: [CardModule],
})
export class AccidentQuickInfo {
  public readonly report = input.required<AccidentReportDto>();
}
