import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { FormGroup } from "@angular/forms";
import { Grid, Stack, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { Converters } from "@/shared/utils";
import { getAccidentSeverityLabel, getAccidentTypeLabel } from "../accident.constants";

@Component({
  selector: "app-accident-review-summary",
  templateUrl: "./accident-review-summary.html",
  imports: [CurrencyFormatPipe, DatePipe, CardModule, TagModule, Grid, Stack, Typography],
})
export class AccidentReviewSummary {
  public readonly incidentForm = input.required<FormGroup>();
  public readonly injuriesDamageForm = input.required<FormGroup>();

  protected readonly locationString = computed(() => {
    const address = this.incidentForm().get("location")?.value ?? null;
    return Converters.addressToString(address) || "-";
  });

  protected readonly typeLabel = computed(() => {
    return getAccidentTypeLabel(this.incidentForm().get("type")?.value);
  });

  protected readonly severityLabel = computed(() => {
    return getAccidentSeverityLabel(this.incidentForm().get("severity")?.value);
  });
}
