import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { Address, TruckDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, TruckStatusTag, TruckTypeTag } from "@/shared/components";
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
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    RouterLink,
    AddressPipe,
    DataContainer,
    TruckStatusTag,
    TruckTypeTag,
    TrucksSummaryStats,
    TrucksFilterPanel,
    TrucksMapView,
  ],
})
export class TrucksList {
  private readonly router = inject(Router);
  private readonly addressPipe = inject(AddressPipe);
  protected readonly store = inject(TrucksListStore);

  protected readonly selectedRow = signal<TruckDto | null>(null);
  protected readonly viewMode = signal<"table" | "map">("table");

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View details",
      icon: "pi pi-eye",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}`),
    },
    {
      label: "Edit truck",
      icon: "pi pi-pencil",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}/edit`),
    },
    {
      label: "Manage documents",
      icon: "pi pi-folder",
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
