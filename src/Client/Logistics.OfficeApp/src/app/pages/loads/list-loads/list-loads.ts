import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {IconFieldModule} from "primeng/iconfield";
import {InputIconModule} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {Observable} from "rxjs";
import {ApiService} from "@/core/api";
import {LoadDto, LoadStatus, PagedResult} from "@/core/api/models";
import {BaseTableComponent, LoadStatusTag, TableQueryParams} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

@Component({
  selector: "app-list-loads",
  templateUrl: "./list-loads.html",
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
    AddressPipe,
    TagModule,
    IconFieldModule,
    InputIconModule,
    FormsModule,
    LoadStatusTag,
  ],
})
export class ListLoadComponent extends BaseTableComponent<LoadDto> {
  private readonly apiService = inject(ApiService);

  protected readonly loadStatus = LoadStatus;
  protected readonly groupByTrip = signal(false);

  protected override query(params: TableQueryParams): Observable<PagedResult<LoadDto>> {
    const sortField = this.apiService.formatSortField(params.sortField, params.sortOrder);

    return this.apiService.loadApi.getLoads({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: sortField || "-DispatchedAt",
      search: params.search,
    });
  }
}
