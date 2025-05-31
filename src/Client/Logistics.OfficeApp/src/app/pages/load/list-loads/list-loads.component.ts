import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {LoadDto, LoadStatus, loadStatusOptions} from "@/core/api/models";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

@Component({
  selector: "app-list-loads",
  templateUrl: "./list-loads.component.html",
  styleUrls: [],
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    DistanceUnitPipe,
    //InvoiceStatusTagComponent,
    AddressPipe,
    TagModule,
    IconFieldModule,
    InputIconModule,
  ],
})
export class ListLoadComponent {
  readonly loadStatus = LoadStatus;
  readonly loads = signal<LoadDto[]>([]);
  readonly isLoading = signal(false);
  readonly totalRecords = signal(0);
  readonly first = signal(0);

  constructor(private readonly apiService: ApiService) {}

  search(event: Event): void {
    this.isLoading.set(true);
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getLoads({search: searchValue}, false).subscribe((result) => {
      if (result.success && result.data) {
        this.loads.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }

  load(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    let sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);

    if (sortField === "") {
      // default sort property
      sortField = "-dispatchedDate";
    }

    this.apiService
      .getLoads({orderBy: sortField, page: page, pageSize: rows}, false)
      .subscribe((result) => {
        if (result.success && result.data) {
          this.loads.set(result.data);
          console.log("Load data:", result.data);

          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }

  getLoadStatusDesc(enumValue: LoadStatus): string {
    return loadStatusOptions.find((option) => option.value === enumValue)?.label ?? "";
  }

  getLoadStatusSeverity(status: LoadStatus): "success" | "info" {
    return status === LoadStatus.Delivered ? "success" : "info";
  }

  getLoadStatusIcon(status: LoadStatus): string {
    return status === LoadStatus.Delivered ? "bi bi-check" : "bi bi-truck";
  }
}
