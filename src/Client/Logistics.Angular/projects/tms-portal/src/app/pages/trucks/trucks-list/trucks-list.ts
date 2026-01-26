import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { Address, TruckDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
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
    AddressPipe,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class TrucksListComponent {
  private readonly router = inject(Router);
  private readonly addressPipe = inject(AddressPipe);
  protected readonly store = inject(TrucksListStore);

  protected readonly selectedRow = signal<TruckDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View details",
      icon: "pi pi-book",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}`),
    },
    {
      label: "Edit truck",
      icon: "pi pi-pen-to-square",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}/edit`),
    },
    {
      label: "Manage documents",
      icon: "pi pi-paperclip",
      command: () => this.router.navigateByUrl(`/trucks/${this.selectedRow()!.id}/documents`),
    },
  ];

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addTruck(): void {
    this.router.navigate(["/trucks/add"]);
  }

  protected formatAddress(address: Address): string {
    return this.addressPipe.transform(address) || "No address provided";
  }
}
