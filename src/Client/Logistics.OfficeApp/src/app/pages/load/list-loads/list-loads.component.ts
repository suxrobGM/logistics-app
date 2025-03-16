import {Component} from "@angular/core";
import {CommonModule} from "@angular/common";
import {RouterLink} from "@angular/router";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {InputTextModule} from "primeng/inputtext";
import {SharedModule} from "primeng/api";
import {CardModule} from "primeng/card";
import {TooltipModule} from "primeng/tooltip";
import {ButtonModule} from "primeng/button";
import {TagModule} from "primeng/tag";
import {InputGroupModule} from 'primeng/inputgroup';
import {InputGroupAddonModule} from 'primeng/inputgroupaddon';
import {LoadStatus, LoadStatusEnum} from "@/core/enums";
import {LoadDto} from "@/core/models";
import {ApiService} from "@/core/services";
import {AddressPipe, DistanceUnitPipe} from "@/core/pipes";
import {PaymentStatusTagComponent} from "@/components";

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
    return LoadStatusEnum.getValue(enumValue).description;
  }

  getLoadStatusSeverity(status: LoadStatus): "success" | "info" {
    return status === LoadStatus.Delivered ? "success" : "info";
  }

  getLoadStatusIcon(status: LoadStatus): string {
    return status === LoadStatus.Delivered ? "bi bi-check" : "bi bi-truck";
  }
}
