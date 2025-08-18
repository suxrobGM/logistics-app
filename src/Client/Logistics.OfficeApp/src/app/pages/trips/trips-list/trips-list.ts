import {CurrencyPipe, DatePipe} from "@angular/common";
import {Component, inject} from "@angular/core";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {Card} from "primeng/card";
import {IconField} from "primeng/iconfield";
import {InputIcon} from "primeng/inputicon";
import {InputText} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {Observable, map} from "rxjs";
import {ApiService} from "@/core/api";
import {PagedResult, TripDto, TripStatus} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {
  BaseTableComponent,
  LoadStatusTag,
  LoadTypeTag,
  TableQueryParams,
  TripStatusTag,
} from "@/shared/components";
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
    CurrencyPipe,
    LoadStatusTag,
    TooltipModule,
    TripStatusTag,
    LoadTypeTag,
  ],
})
export class TripsList extends BaseTableComponent<TripDto> {
  protected readonly tripStatus = TripStatus;

  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

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

  protected askRemoveTrip(trip: TripDto): void {
    if (trip.status !== TripStatus.Draft) {
      this.toastService.showError("Only draft trips can be deleted");
      return;
    }

    this.toastService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(trip.id),
    });
  }

  private deleteTrip(tripId: string): void {
    this.apiService.tripApi.deleteTrip(tripId).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Trip deleted successfully");
        this.data.update((trips) => trips.filter((trip) => trip.id !== tripId));
      }
    });
  }
}
