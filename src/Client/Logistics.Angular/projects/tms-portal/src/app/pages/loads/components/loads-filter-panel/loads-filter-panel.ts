import { Component, computed, input, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { CustomerDto, LoadStatus, LoadType, TruckDto } from "@logistics/shared/api";
import { loadStatusOptions, loadTypeOptions } from "@logistics/shared/api/enums";
import type { SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { MultiSelectModule } from "primeng/multiselect";
import { DateRangePicker, LabeledField, SearchCustomer, SearchInput, SearchTruck } from "@/shared/components";

export interface LoadsFilterState {
  statuses: LoadStatus[];
  types: LoadType[];
  truck: TruckDto | null;
  customer: CustomerDto | null;
  onlyActive: boolean;
  dateRange: Date[] | null;
}

@Component({
  selector: "app-loads-filter-panel",
  templateUrl: "./loads-filter-panel.html",
  imports: [
    CardModule,
    ButtonModule,
    FormsModule,
    MultiSelectModule,
    CheckboxModule,
    SearchInput,
    SearchTruck,
    SearchCustomer,
    DateRangePicker,
    LabeledField,
  ],
})
export class LoadsFilterPanel {
  public readonly isLoading = input(false);
  public readonly filtersChanged = output<LoadsFilterState>();
  public readonly searchChanged = output<string>();

  // Filter state
  protected readonly selectedStatuses = signal<LoadStatus[]>([]);
  protected readonly selectedTypes = signal<LoadType[]>([]);
  protected readonly selectedTruck = signal<TruckDto | null>(null);
  protected readonly selectedCustomer = signal<CustomerDto | null>(null);
  protected readonly onlyActiveLoads = signal<boolean>(false);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Filter options
  protected readonly statusOptions: SelectItem[] = loadStatusOptions;
  protected readonly typeOptions: SelectItem[] = loadTypeOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedTypes().length > 0) count++;
    if (this.selectedTruck()) count++;
    if (this.selectedCustomer()) count++;
    if (this.onlyActiveLoads()) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  protected onSearch(value: string): void {
    this.searchChanged.emit(value);
  }

  protected applyFilters(): void {
    this.filtersChanged.emit({
      statuses: this.selectedStatuses(),
      types: this.selectedTypes(),
      truck: this.selectedTruck(),
      customer: this.selectedCustomer(),
      onlyActive: this.onlyActiveLoads(),
      dateRange: this.dateRange(),
    });
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedTypes.set([]);
    this.selectedTruck.set(null);
    this.selectedCustomer.set(null);
    this.onlyActiveLoads.set(false);
    this.dateRange.set(null);
    this.filtersChanged.emit({
      statuses: [],
      types: [],
      truck: null,
      customer: null,
      onlyActive: false,
      dateRange: null,
    });
  }

  protected onDateRangeChange(dates: Date[]): void {
    this.dateRange.set(dates);
    this.applyFilters();
  }
}
