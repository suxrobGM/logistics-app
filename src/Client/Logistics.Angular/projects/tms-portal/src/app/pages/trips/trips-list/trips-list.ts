import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteTrip, dispatchTrip, cancelTrip } from "@logistics/shared/api";
import type { TripDto, TripStatus, TruckDto } from "@logistics/shared/api";
import { tripStatusOptions } from "@logistics/shared/api/enums";
import { AddressPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { Button } from "primeng/button";
import { Card } from "primeng/card";
import { Checkbox } from "primeng/checkbox";
import { MenuModule } from "primeng/menu";
import { MultiSelect } from "primeng/multiselect";
import { ProgressBarModule } from "primeng/progressbar";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import {
  DataContainer,
  DateRangePicker,
  LabeledField,
  LoadStatusTag,
  LoadTypeTag,
  SearchInput,
  SearchTruck,
  TripStatusTag,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
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
    DateRangePicker,
    SearchTruck,
    SearchInput,
    LabeledField,
    ProgressBarModule,
  ],
})
export class TripsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  protected readonly store = inject(TripsListStore);

  protected readonly selectedRow = signal<TripDto | null>(null);
  protected readonly isProcessing = signal<boolean>(false);

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

  // Dynamic action menu items based on selected trip status
  protected readonly actionMenuItems = computed<MenuItem[]>(() => {
    const trip = this.selectedRow();
    const items: MenuItem[] = [
      {
        label: "View trip details",
        icon: "pi pi-eye",
        command: () => this.router.navigate(["/trips", trip!.id]),
      },
    ];

    // Edit only available for draft trips
    if (trip?.status === "draft") {
      items.push({
        label: "Edit trip details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigate(["/trips", trip!.id, "edit"]),
      });
    }

    // Dispatch available for draft trips with truck assigned
    if (trip?.status === "draft" && trip?.truckId) {
      items.push({
        label: "Dispatch trip",
        icon: "pi pi-send",
        command: () => this.askDispatchTrip(trip!),
      });
    }

    // Cancel available for non-completed/cancelled trips
    if (trip?.status === "draft" || trip?.status === "dispatched" || trip?.status === "in_transit") {
      items.push({
        label: "Cancel trip",
        icon: "pi pi-times",
        command: () => this.askCancelTrip(trip!),
      });
    }

    // Delete only available for draft trips
    if (trip?.status === "draft") {
      items.push({
        separator: true,
      });
      items.push({
        label: "Delete trip",
        icon: "pi pi-trash",
        styleClass: "text-red-500",
        command: () => this.askRemoveTrip(trip!),
      });
    }

    return items;
  });

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

  protected onDateRangeChange(dates: Date[]): void {
    this.dateRange.set(dates);
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

  protected askDispatchTrip(trip: TripDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to dispatch trip "${trip.name}"?`,
      accept: () => this.doDispatchTrip(trip.id!),
    });
  }

  private async doDispatchTrip(tripId: string): Promise<void> {
    this.isProcessing.set(true);
    try {
      await this.api.invoke(dispatchTrip, { tripId });
      this.toastService.showSuccess("Trip dispatched successfully");
      this.store.load();
    } catch {
      this.toastService.showError("Failed to dispatch trip");
    } finally {
      this.isProcessing.set(false);
    }
  }

  protected askCancelTrip(trip: TripDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to cancel trip "${trip.name}"? This action cannot be undone.`,
      accept: () => this.doCancelTrip(trip.id!),
    });
  }

  private async doCancelTrip(tripId: string): Promise<void> {
    this.isProcessing.set(true);
    try {
      await this.api.invoke(cancelTrip, { tripId, body: { tripId } });
      this.toastService.showSuccess("Trip cancelled successfully");
      this.store.load();
    } catch {
      this.toastService.showError("Failed to cancel trip");
    } finally {
      this.isProcessing.set(false);
    }
  }

  // Calculate progress percentage for a trip
  protected getProgressPercentage(trip: TripDto): number {
    const stops = trip.stops ?? [];
    if (stops.length === 0) return 0;
    const completed = stops.filter((s) => s.arrivedAt).length;
    return Math.round((completed / stops.length) * 100);
  }
}
