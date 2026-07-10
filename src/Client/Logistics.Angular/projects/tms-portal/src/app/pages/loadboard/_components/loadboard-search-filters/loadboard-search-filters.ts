import { Component, computed, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, max, min } from "@angular/forms/signals";
import { type SearchLoadBoardCommand } from "@logistics/shared/api";
import {
  Grid,
  Stack,
  UiDateField,
  UiMultiSelectField,
  UiNumberField,
  UiTextField,
} from "@logistics/shared/components";
import { LocalizationService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { UiFormField } from "@/shared/components";
import { EQUIPMENT_OPTIONS } from "../loadboard.constants";

@Component({
  selector: "app-loadboard-search-filters",
  templateUrl: "./loadboard-search-filters.html",
  imports: [
    ButtonModule,
    FormRoot,
    FormField,
    UiFormField,
    Grid,
    UiDateField,
    UiNumberField,
    UiTextField,
    UiMultiSelectField,
    Stack,
  ],
})
export class LoadBoardSearchFilters {
  private readonly localization = inject(LocalizationService, { optional: true });

  public readonly searching = input(false);
  public readonly searched = output<SearchLoadBoardCommand>();

  protected readonly equipmentOptions = EQUIPMENT_OPTIONS;
  protected readonly distanceUnitLabel = computed(
    () => this.localization?.getDistanceUnitLabel() ?? "mi",
  );

  protected readonly model = signal({
    originCity: "",
    originState: "",
    originRadius: 50,
    destinationCity: "",
    destinationState: "",
    destinationRadius: 50,
    pickupDateStart: null as Date | null,
    pickupDateEnd: null as Date | null,
    equipmentTypes: [] as string[],
    maxResults: 50,
  });

  /**
   * `[formRoot]` runs `submission.action` on submit. It marks the whole tree touched first, skips
   * the action while invalid, and drives `form().submitting()`. The radius / max-results bounds are
   * schema validators so Signal Forms drives the reserved `min` / `max` state inputs on the number
   * fields (which is why the template no longer sets `[min]` / `[max]`).
   */
  protected readonly form = form(
    this.model,
    (p) => {
      min(p.originRadius, 0, { message: "Origin radius cannot be negative." });
      max(p.originRadius, 500, { message: "Origin radius cannot exceed 500." });
      min(p.destinationRadius, 0, { message: "Destination radius cannot be negative." });
      max(p.destinationRadius, 500, { message: "Destination radius cannot exceed 500." });
      min(p.maxResults, 10, { message: "Show at least 10 results." });
      max(p.maxResults, 100, { message: "Show at most 100 results." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          this.searched.emit({
            originCity: v.originCity || undefined,
            originState: v.originState || undefined,
            originRadius: v.originRadius || undefined,
            destinationCity: v.destinationCity || undefined,
            destinationState: v.destinationState || undefined,
            destinationRadius: v.destinationRadius || undefined,
            pickupDateStart: v.pickupDateStart?.toISOString(),
            pickupDateEnd: v.pickupDateEnd?.toISOString(),
            equipmentTypes: v.equipmentTypes.length ? v.equipmentTypes : undefined,
            maxResults: v.maxResults || 50,
          } as SearchLoadBoardCommand);
          return undefined;
        },
      },
    },
  );
}
