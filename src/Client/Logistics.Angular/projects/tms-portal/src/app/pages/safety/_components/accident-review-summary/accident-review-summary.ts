import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { FieldTree } from "@angular/forms/signals";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Badge, Card, Grid, InfoItem, InfoRow, Stack } from "@logistics/shared/ui";
import { Converters } from "@/shared/utils";
import {
  getAccidentSeverityLabel,
  getAccidentTypeLabel,
  type AccidentIncidentModel,
  type AccidentInjuriesDamageModel,
} from "../accident.constants";

@Component({
  selector: "app-accident-review-summary",
  templateUrl: "./accident-review-summary.html",
  imports: [Badge, Card, CurrencyFormatPipe, DatePipe, Grid, InfoItem, InfoRow, Stack],
})
export class AccidentReviewSummary {
  public readonly incidentForm = input.required<FieldTree<AccidentIncidentModel>>();
  public readonly injuriesDamageForm = input.required<FieldTree<AccidentInjuriesDamageModel>>();

  protected readonly locationString = computed(() => {
    const address = this.incidentForm().location().value() ?? null;
    return Converters.addressToString(address) || "-";
  });

  protected readonly typeLabel = computed(() => {
    return getAccidentTypeLabel(this.incidentForm().type().value());
  });

  protected readonly severityLabel = computed(() => {
    return getAccidentSeverityLabel(this.incidentForm().severity().value());
  });
}
