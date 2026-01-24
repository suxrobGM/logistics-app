import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteTrip } from "@logistics/shared/api";
import { tripStatusOptions } from "@logistics/shared/api/enums";
import type { TripDto, TripStatus, TruckDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { Button } from "primeng/button";
import { Card } from "primeng/card";
import { Checkbox } from "primeng/checkbox";
import { DatePicker } from "primeng/datepicker";
import { MenuModule } from "primeng/menu";
import { MultiSelect } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import {
  DataContainer,
  LabeledField,
  LoadStatusTag,
  LoadTypeTag,
  SearchInput,
  SearchTruck,
  TripStatusTag,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { type DatePreset, getDatePreset } from "@/shared/utils";
import { TripsListStore } from "../store/trips-list.store";

@Component({
  selector: "app-trips-list",
  templateUrl: "./trips-list.html",
  providers: [TripsListStore],
  imports: [
    Button,
    RouterLink,
    Card,
    TableModule,
    FormsModule,
    DatePipe,
    DistanceUnitPipe,
    AddressPipe,
    CurrencyPipe,
    LoadStatusTag,
    TooltipModule,
    TripStatusTag,
    LoadTypeTag,
    MenuModule,
    DataContainer,
    MultiSelect,
    Checkbox,
    DatePicker,
    SearchTruck,
    SearchInput,
    LabeledField,
  ],
})
export class TripsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  protected readonly store = inject(TripsListStore);

  protected readonly selectedRow = signal<TripDto | null>(null);
  protected readonly actionMenuItems: MenuItem[];

  // Filter options
  protected readonly statusOptions = tripStatusOptions;

  // Filter state (signals for reactivity)
  protected readonly selectedStatuses = signal<TripStatus[]>([]);
  protected readonly selectedTruck = signal<TruckDto | null>(null);
  protected readonly dateRange = signal<Date[] | null>(null);
  protected readonly onlyActiveTrips = signal<boolean>(false);

  // Computed active filter count
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedTruck()) count++;
    if (this.dateRange()?.length === 2) count++;
    if (this.onlyActiveTrips()) count++;
    return count;
  });

  constructor() {
    this.actionMenuItems = [
      {
        label: "View trip details",
        icon: "pi pi-eye",
        command: () => this.router.navigate(["/trips", this.selectedRow()!.id]),
      },
      {
        label: "Edit trip details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigate(["/trips", this.selectedRow()!.id, "edit"]),
      },
      {
        label: "Delete trip",
        icon: "pi pi-trash",
        command: () => this.askRemoveTrip(this.selectedRow()!),
      },
    ];
  }

  protected onSearch(search: string): void {
    this.store.setSearch(search);
  }

  protected applyFilters(): void {
    const statuses = this.selectedStatuses();
    const truck = this.selectedTruck();
    const range = this.dateRange();
    this.store.setFilters({
      Status: statuses.length === 1 ? statuses[0] : undefined,
      TruckId: truck?.id,
      StartDate: range?.[0]?.toISOString(),
      EndDate: range?.[1]?.toISOString(),
      OnlyActiveTrips: this.onlyActiveTrips() || undefined,
    });
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedTruck.set(null);
    this.dateRange.set(null);
    this.onlyActiveTrips.set(false);
    this.store.setFilters({});
  }

  protected setDatePreset(preset: DatePreset): void {
    this.dateRange.set(getDatePreset(preset));
    this.applyFilters();
  }

  protected addTrip(): void {
    this.router.navigate(["/trips/add"]);
  }

  protected askRemoveTrip(trip: TripDto): void {
    if (trip.status !== "draft") {
      this.toastService.showError("Only draft trips can be deleted");
      return;
    }

    this.toastService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(trip.id!),
    });
  }

  private async deleteTrip(tripId: string): Promise<void> {
    await this.api.invoke(deleteTrip, { id: tripId });
    this.toastService.showSuccess("Trip deleted successfully");
    this.store.removeItem(tripId);
  }
}
