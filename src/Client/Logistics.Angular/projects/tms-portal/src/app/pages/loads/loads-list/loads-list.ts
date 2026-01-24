import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { loadStatusOptions, loadTypeOptions } from "@logistics/shared/api/enums";
import type {
  CustomerDto,
  LoadDto,
  LoadStatus,
  LoadType,
  TruckDto,
} from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import type { MenuItem, SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { DatePickerModule } from "primeng/datepicker";
import { MenuModule } from "primeng/menu";
import { MultiSelectModule } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import {
  DataContainer,
  LabeledField,
  LoadStatusTag,
  LoadTypeTag,
  SearchCustomer,
  SearchInput,
  SearchTruck,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { type DatePreset, getDatePreset } from "@/shared/utils";
import { LoadsListStore } from "../store/loads-list.store";

@Component({
  selector: "app-loads-list",
  templateUrl: "./loads-list.html",
  providers: [LoadsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    DistanceUnitPipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
    MenuModule,
    DataContainer,
    DatePipe,
    CurrencyPipe,
    SearchInput,
    FormsModule,
    CheckboxModule,
    DatePickerModule,
    MultiSelectModule,
    SearchTruck,
    SearchCustomer,
    LabeledField,
  ],
})
export class LoadsListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(LoadsListStore);

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

  protected readonly selectedRow = signal<LoadDto | null>(null);
  protected readonly actionMenuItems: MenuItem[];

  constructor() {
    this.actionMenuItems = [
      {
        label: "View details",
        icon: "pi pi-eye",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}`),
      },
      {
        label: "Edit load details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}/edit`),
      },
      {
        label: "View truck details",
        icon: "pi pi-truck",
        command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.assignedTruckId}`),
      },
      {
        label: "View invoices",
        icon: "pi pi-book",
        command: () => this.router.navigateByUrl(`/invoices/loads/${this.selectedRow()!.id}`),
      },
    ];
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addLoad(): void {
    this.router.navigate(["/loads/add"]);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    // Status filter (take first if multiple - API doesn't support multiple)
    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Status"] = statuses[0];
    }

    // Type filter (take first if multiple - API doesn't support multiple)
    const types = this.selectedTypes();
    if (types.length > 0) {
      filters["Type"] = types[0];
    }

    // Truck filter
    const truck = this.selectedTruck();
    if (truck?.id) {
      filters["TruckId"] = truck.id;
    }

    // Customer filter
    const customer = this.selectedCustomer();
    if (customer?.id) {
      filters["CustomerId"] = customer.id;
    }

    // Active only filter
    filters["OnlyActiveLoads"] = this.onlyActiveLoads();

    // Date range filter
    const range = this.dateRange();
    if (range?.length === 2) {
      filters["StartDate"] = range[0].toISOString();
      filters["EndDate"] = range[1].toISOString();
    }

    this.store.setFilters(filters);
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedTypes.set([]);
    this.selectedTruck.set(null);
    this.selectedCustomer.set(null);
    this.onlyActiveLoads.set(false);
    this.dateRange.set(null);
    this.store.setFilters({});
  }

  protected setDatePreset(preset: DatePreset): void {
    this.dateRange.set(getDatePreset(preset));
    this.applyFilters();
  }
}
