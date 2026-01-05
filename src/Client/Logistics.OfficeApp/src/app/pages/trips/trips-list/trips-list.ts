import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { MenuItem } from "primeng/api";
import { Button } from "primeng/button";
import { Card } from "primeng/card";
import { IconField } from "primeng/iconfield";
import { InputIcon } from "primeng/inputicon";
import { InputText } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getTrips$Json, deleteTrip$Json } from "@/core/api";
import { TripDto, TripStatus } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { LoadStatusTag, LoadTypeTag, TripStatusTag } from "@/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@/shared/pipes";

@Component({
  selector: "app-trips-list",
  templateUrl: "./trips-list.html",
  imports: [
    Button,
    RouterLink,
    Card,
    TableModule,
    IconField,
    InputIcon,
    DatePipe,
    DistanceUnitPipe,
    InputText,
    AddressPipe,
    CurrencyPipe,
    LoadStatusTag,
    TooltipModule,
    TripStatusTag,
    LoadTypeTag,
    MenuModule,
  ],
})
export class TripsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly data = signal<TripDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);
  protected readonly tripStatus = TripStatus;
  protected readonly actionMenuItems: MenuItem[];
  protected readonly selectedRow = signal<TripDto | null>(null);

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

  protected async onLazyLoad(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const rows = event.rows ?? 10;
    const page = (event.first ?? 0) / rows;
    const orderBy = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getTrips$Json, {
      Page: page + 1,
      PageSize: rows,
      OrderBy: orderBy,
    });

    if (result.data) {
      console.log("Trips data:", result.data);
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(page * rows);
    }

    this.isLoading.set(false);
  }

  protected async onSearch(event: Event): Promise<void> {
    this.isLoading.set(true);
    const value = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getTrips$Json, {
      Search: value,
      Page: 1,
      PageSize: 10,
    });

    if (result.data) {
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(0);
    }

    this.isLoading.set(false);
  }

  protected askRemoveTrip(trip: TripDto): void {
    if (trip.status !== TripStatus.Draft) {
      this.toastService.showError("Only draft trips can be deleted");
      return;
    }

    this.toastService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(trip.id!),
    });
  }

  private async deleteTrip(tripId: string): Promise<void> {
    const result = await this.api.invoke(deleteTrip$Json, { id: tripId });
    if (result.success) {
      this.toastService.showSuccess("Trip deleted successfully");
      this.data.update((trips) => trips.filter((trip) => trip.id !== tripId));
    }
  }
}
