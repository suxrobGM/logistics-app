import {Component, OnInit, inject, input, signal} from "@angular/core";
import {Router} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {CardModule} from "primeng/card";
import {ApiService} from "@/core/api";
import {TripLoadDto, TripStopDto, UpdateTripCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {TripForm, TripFormValue} from "../components";

@Component({
  selector: "app-trip-edit",
  templateUrl: "./trip-edit.html",
  imports: [CardModule, TripForm],
})
export class TripEditPage implements OnInit {
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly apiService = inject(ApiService);
  private readonly router = inject(Router);

  protected readonly tripId = input<string>();

  protected readonly tripLoads = signal<TripLoadDto[]>([]);
  protected readonly tripStops = signal<TripStopDto[]>([]);
  protected readonly tripNumber = signal<number | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly initialData = signal<Partial<TripFormValue> | null>(null);

  ngOnInit(): void {
    this.fetchTrip();
  }

  protected confirmToDelete(): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(),
    });
  }

  protected updateTrip(formValues: TripFormValue): void {
    //this.isLoading.set(true);
    // const command: UpdateTripCommand = {
    //   ...formValues,
    //   existingLoadIds: this.existingLoadIds,
    // };
    // this.apiService.tripApi.updateTrip(this.tripId(), command).subscribe((result) => {
    //   if (result.success) {
    //     this.toastService.showSuccess("Trip updated successfully");
    //     this.fetchTrip();
    //   }
    //   this.isLoading.set(false);
    // });
  }

  private fetchTrip(): void {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);

    this.apiService.tripApi.getTrip(tripId).subscribe((result) => {
      if (result.success && result.data) {
        const trip = result.data;

        this.initialData.set({
          name: trip.name,
          plannedStart: new Date(trip.plannedStart),
          //deliveryCost: trip.loads.reduce((sum, load) => sum + (load.deliveryCost || 0), 0),
          //distance: trip.loads.reduce((sum, load) => sum + (load.distance || 0), 0),
          truckId: trip.truckId,
        });

        this.tripLoads.set(trip.loads);
        this.tripStops.set(trip.stops);
        this.tripNumber.set(trip.number);
      }

      this.isLoading.set(false);
    });
  }

  private deleteTrip(): void {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);

    this.apiService.tripApi.deleteTrip(tripId).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A trip has been deleted successfully");
        this.router.navigateByUrl("/trips");
      }

      this.isLoading.set(false);
    });
  }
}
