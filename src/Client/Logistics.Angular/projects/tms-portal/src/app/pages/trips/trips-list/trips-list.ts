import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import {
  DataContainer,
  LoadStatusTag,
  LoadTypeTag,
  TripStatusTag,
} from "@logistics/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { Button } from "primeng/button";
import { Card } from "primeng/card";
import { IconField } from "primeng/iconfield";
import { InputIcon } from "primeng/inputicon";
import { InputText } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, deleteTrip } from "@/core/api";
import type { TripDto } from "@/core/api/models";
import { ToastService } from "@/core/services";
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
    DataContainer,
  ],
})
export class TripsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  protected readonly store = inject(TripsListStore);

  protected readonly selectedRow = signal<TripDto | null>(null);
  protected readonly actionMenuItems: MenuItem[];

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

  protected onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.store.setSearch(value);
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
