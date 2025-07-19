import {DatePipe} from "@angular/common";
import {Component, inject} from "@angular/core";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {Card} from "primeng/card";
import {IconField} from "primeng/iconfield";
import {InputIcon} from "primeng/inputicon";
import {InputText} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {Observable} from "rxjs";
import {ApiService} from "@/core/api";
import {PagedResult, TripDto} from "@/core/api/models";
import {BaseTableComponent, TableQueryParams} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

@Component({
  selector: "app-trips-list",
  templateUrl: "./trips-list.html",
  imports: [
    Button,
    RouterLink,
    Card,
    TableModule,
    IconField,
    InputIcon,
    DatePipe,
    DistanceUnitPipe,
    InputText,
    AddressPipe,
  ],
})
export class TripsList extends BaseTableComponent<TripDto> {
  private readonly apiService = inject(ApiService);

  protected query(params: TableQueryParams): Observable<PagedResult<TripDto>> {
    const orderBy = this.apiService.formatSortField(params.sortField, params.sortOrder);
    return this.apiService.tripApi.getTrips({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: orderBy,
      search: params.search,
    });
  }
}
