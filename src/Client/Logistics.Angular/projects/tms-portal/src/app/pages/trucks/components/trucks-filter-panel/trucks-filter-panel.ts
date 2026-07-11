import { Component, computed, input, output, signal } from "@angular/core";
import type { TruckStatus, TruckType } from "@logistics/shared/api";
import { truckStatusOptions, truckTypeOptions } from "@logistics/shared/api/enums";
import { Card, Stack, Typography, UiButton, UiMultiSelectField } from "@logistics/shared/ui";
import { SearchField, UiFormField } from "@/shared/components";

export interface TrucksFilterState {
  statuses: TruckStatus[];
  types: TruckType[];
}

@Component({
  selector: "app-trucks-filter-panel",
  templateUrl: "./trucks-filter-panel.html",
  imports: [Card, SearchField, Stack, Typography, UiButton, UiFormField, UiMultiSelectField],
})
export class TrucksFilterPanel {
  public readonly isLoading = input(false);
  public readonly filtersChanged = output<TrucksFilterState>();
  public readonly searchChanged = output<string>();

  // Filter state
  protected readonly selectedStatuses = signal<TruckStatus[]>([]);
  protected readonly selectedTypes = signal<TruckType[]>([]);

  // Filter options
  protected readonly statusOptions = truckStatusOptions;
  protected readonly typeOptions = truckTypeOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedTypes().length > 0) count++;
    return count;
  });

  protected onSearch(value: string): void {
    this.searchChanged.emit(value);
  }

  protected applyFilters(): void {
    this.filtersChanged.emit({
      statuses: this.selectedStatuses(),
      types: this.selectedTypes(),
    });
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedTypes.set([]);
    this.filtersChanged.emit({
      statuses: [],
      types: [],
    });
  }
}
