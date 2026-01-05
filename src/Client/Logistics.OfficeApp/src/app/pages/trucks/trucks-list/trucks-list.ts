import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { SharedModule } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getTrucks$Json } from "@/core/api";
import type { AddressDto, TruckDto } from "@/core/api/models";
import { AddressPipe } from "@/shared/pipes";

@Component({
  selector: "app-trucks-list",
  templateUrl: "./trucks-list.html",
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    AddressPipe,
    IconFieldModule,
    InputIconModule,
  ],
  providers: [AddressPipe],
})
export class TrucksListComponent {
  private readonly api = inject(Api);
  private readonly addressPipe = inject(AddressPipe);

  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);

  protected async search(event: Event): Promise<void> {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getTrucks$Json, { Search: searchValue });
    if (result.success && result.data) {
      this.trucks.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  protected async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getTrucks$Json, {
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
    });
    if (result.success && result.data) {
      this.trucks.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }

  protected formatAddress(address: AddressDto): string {
    return this.addressPipe.transform(address) || "No address provided";
  }
}
