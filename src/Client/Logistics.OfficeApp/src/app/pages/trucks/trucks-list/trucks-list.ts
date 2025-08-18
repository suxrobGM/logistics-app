import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {AddressDto, TruckDto} from "@/core/api/models";
import {AddressPipe} from "@/shared/pipes";

@Component({
  selector: "app-trucks-list",
  templateUrl: "./trucks-list.html",
  imports: [
    CommonModule,
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
  private readonly apiService = inject(ApiService);
  private readonly addressPipe = inject(AddressPipe);

  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);

  protected search(event: Event): void {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.truckApi.getTrucks({search: searchValue}).subscribe((result) => {
      if (result.success && result.data) {
        this.trucks.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }

  protected load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.formatSortField(event.sortField as string, event.sortOrder);

    this.apiService.truckApi
      .getTrucks({orderBy: sortField, page: page, pageSize: rows})
      .subscribe((result) => {
        if (result.success && result.data) {
          this.trucks.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }

  protected formatAddress(address: AddressDto): string {
    return this.addressPipe.transform(address) || "No address provided";
  }
}
