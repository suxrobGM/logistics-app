import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import {
  DataContainer,
  LoadStatusTag,
  LoadTypeTag,
  SearchInput,
} from "@logistics/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import type { LoadDto } from "@/core/api/models";
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
  ],
})
export class LoadsListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(LoadsListStore);

  protected readonly selectedRow = signal<LoadDto | null>(null);
  protected readonly actionMenuItems: MenuItem[];

  constructor() {
    this.actionMenuItems = [
      {
        label: "Edit load details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}/edit`),
      },
      {
        label: "Manage documents",
        icon: "pi pi-paperclip",
        command: () => this.router.navigateByUrl(`/loads/${this.selectedRow()!.id}/documents`),
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
}
