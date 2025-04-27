import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputGroupModule} from "primeng/inputgroup";
import {InputGroupAddonModule} from "primeng/inputgroupaddon";
import {InputTextModule} from "primeng/inputtext";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {PaymentStatusTagComponent} from "@/components";
import {ApiService} from "@/core/api";
import {LoadDto, LoadStatus, loadStatusOptions} from "@/core/api/models";
import {AddressPipe, DistanceUnitPipe} from "@/core/pipes";

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
    PaymentStatusTagComponent,
    AddressPipe,
    TagModule,
    InputGroupModule,
    InputGroupAddonModule,
  ],
})
export class ListLoadComponent {
  public loadStatus = LoadStatus;
  public loads: LoadDto[] = [];
  public isLoading = false;
  public totalRecords = 0;
  public first = 0;

  constructor(private readonly apiService: ApiService) {}

  search(event: Event) {
    this.isLoading = true;
    const searchValue = (event.target as HTMLInputElement).value;

    this.apiService.getLoads({search: searchValue}, false).subscribe((result) => {
      if (result.success && result.data) {
        this.loads = result.data;
        this.totalRecords = result.totalItems;
      }

      this.isLoading = false;
    });
  }

  load(event: TableLazyLoadEvent) {
    this.isLoading = true;
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
          this.loads = result.data;
          this.totalRecords = result.totalItems;
        }

        this.isLoading = false;
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
