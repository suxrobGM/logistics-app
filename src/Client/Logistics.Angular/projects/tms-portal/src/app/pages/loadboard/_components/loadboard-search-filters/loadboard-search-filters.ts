import { Component, computed, inject, input, output } from "@angular/core";
import { FormBuilder, ReactiveFormsModule } from "@angular/forms";
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
    UiFormField,
    Grid,
    UiDateField,
    UiNumberField,
    UiTextField,
    UiMultiSelectField,
    ReactiveFormsModule,
    Stack,
  ],
})
export class LoadBoardSearchFilters {
  private readonly fb = inject(FormBuilder);
  private readonly localization = inject(LocalizationService, { optional: true });

  public readonly searching = input(false);
  public readonly searched = output<SearchLoadBoardCommand>();

  protected readonly equipmentOptions = EQUIPMENT_OPTIONS;
  protected readonly distanceUnitLabel = computed(
    () => this.localization?.getDistanceUnitLabel() ?? "mi",
  );

  protected readonly form = this.fb.group({
    originCity: [""],
    originState: [""],
    originRadius: [50],
    destinationCity: [""],
    destinationState: [""],
    destinationRadius: [50],
    pickupDateStart: [null as Date | null],
    pickupDateEnd: [null as Date | null],
    equipmentTypes: [[] as string[]],
    maxResults: [50],
  });

  protected submit(): void {
    const v = this.form.value;
    this.searched.emit({
      originCity: v.originCity || undefined,
      originState: v.originState || undefined,
      originRadius: v.originRadius || undefined,
      destinationCity: v.destinationCity || undefined,
      destinationState: v.destinationState || undefined,
      destinationRadius: v.destinationRadius || undefined,
      pickupDateStart: v.pickupDateStart?.toISOString(),
      pickupDateEnd: v.pickupDateEnd?.toISOString(),
      equipmentTypes: v.equipmentTypes?.length ? v.equipmentTypes : undefined,
      maxResults: v.maxResults || 50,
    } as SearchLoadBoardCommand);
  }
}
