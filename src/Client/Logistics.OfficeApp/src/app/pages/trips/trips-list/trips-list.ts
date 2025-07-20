import {DatePipe} from "@angular/common";
import {Component, inject} from "@angular/core";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {Card} from "primeng/card";
import {IconField} from "primeng/iconfield";
import {InputIcon} from "primeng/inputicon";
import {InputText} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {Tag} from "primeng/tag";
import {Observable, map} from "rxjs";
import {ApiService} from "@/core/api";
import {PagedResult, TripDto, TripStatus} from "@/core/api/models";
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
    Tag,
  ],
})
export class TripsList extends BaseTableComponent<TripDto> {
  private readonly apiService = inject(ApiService);

  protected query(params: TableQueryParams): Observable<PagedResult<TripDto>> {
    const orderBy = this.apiService.formatSortField(params.sortField, params.sortOrder);
    return this.apiService.tripApi
      .getTrips({
        page: params.page + 1,
        pageSize: params.size,
        orderBy: orderBy,
        search: params.search,
      })
      .pipe(
        map((result) => {
          console.log("Trips data:", result.data);
          return result;
        })
      );
  }

  tripStatusColor(status?: TripStatus | null): string | null {
    switch (status) {
      case TripStatus.Dispatched:
        return "info";
      case TripStatus.Completed:
        return "success";
      case TripStatus.InTransit:
        return "info";
      case TripStatus.Planned:
        return "warning";
      case TripStatus.Cancelled:
        return "danger";
      default:
        return null;
    }
  }
}
