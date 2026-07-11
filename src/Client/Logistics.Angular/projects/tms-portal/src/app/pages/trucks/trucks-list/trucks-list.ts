import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { Address, TruckDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import {
  Card,
  Stack,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  UiTooltip,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { DataContainer, PageHeader, TruckStatusTag, TruckTypeTag } from "@/shared/components";
import {
  TrucksFilterPanel,
  type TrucksFilterState,
} from "../components/trucks-filter-panel/trucks-filter-panel";
import { TrucksMapView } from "../components/trucks-map-view/trucks-map-view";
import { TrucksSummaryStats } from "../components/trucks-summary-stats/trucks-summary-stats";
import { TrucksListStore } from "../store/trucks-list.store";

@Component({
  selector: "app-trucks-list",
  templateUrl: "./trucks-list.html",
  providers: [TrucksListStore, AddressPipe],
  imports: [
    AddressPipe,
    Card,
    DataContainer,
    PageHeader,
    Stack,
    TrucksFilterPanel,
    TrucksMapView,
    TrucksSummaryStats,
    TruckStatusTag,
    TruckTypeTag,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
    UiTooltip,
  ],
})
export class TrucksList {
  private readonly router = inject(Router);
  private readonly addressPipe = inject(AddressPipe);
  protected readonly store = inject(TrucksListStore);

  protected readonly selectedRow = signal<TruckDto | null>(null);
  protected readonly viewMode = signal<"table" | "map">("table");

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "View details",
      icon: "eye",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}`),
    },
    {
      label: "Edit truck",
      icon: "pencil",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}/edit`),
    },
    {
      label: "Manage documents",
      icon: "folder",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}/documents`),
    },
  ];

  protected onSearchChanged(value: string): void {
    this.store.setSearch(value);
  }

  protected onFiltersChanged(filters: TrucksFilterState): void {
    this.store.setFilters({
      Statuses: filters.statuses.length > 0 ? filters.statuses : undefined,
      Types: filters.types.length > 0 ? filters.types : undefined,
    });
  }

  protected addTruck(): void {
    this.router.navigate(["/trucks/add"]);
  }

  protected formatAddress(address: Address): string {
    return this.addressPipe.transform(address) || "No address provided";
  }
}
