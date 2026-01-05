import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import type { LoadDto } from "@/core/api/models";
import { DataContainer, LoadStatusTag, LoadTypeTag } from "@/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@/shared/pipes";
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
    InputTextModule,
    DistanceUnitPipe,
    AddressPipe,
    IconFieldModule,
    InputIconModule,
    LoadStatusTag,
    LoadTypeTag,
    MenuModule,
    DataContainer,
    DatePipe,
    CurrencyPipe,
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

  protected onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.store.setSearch(value);
  }

  protected addLoad(): void {
    this.router.navigate(["/loads/add"]);
  }
}
