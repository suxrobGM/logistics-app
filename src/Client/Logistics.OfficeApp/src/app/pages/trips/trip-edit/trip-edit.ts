import {Component, OnInit, inject, input, signal} from "@angular/core";
import {Router} from "@angular/router";
import {CardModule} from "primeng/card";
import {ApiService} from "@/core/api";
import {TripStatus, UpdateTripCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {TripForm, TripFormValue} from "../components";

@Component({
  selector: "app-trip-edit",
  templateUrl: "./trip-edit.html",
  imports: [CardModule, TripForm],
})
export class TripEditPage implements OnInit {
  private readonly toastService = inject(ToastService);
  private readonly apiService = inject(ApiService);
  private readonly router = inject(Router);

  protected readonly tripId = input<string>();
  protected readonly disabledForEditing = signal(false);

  protected readonly tripNumber = signal<number | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly initialData = signal<Partial<TripFormValue> | null>(null);

  ngOnInit(): void {
    this.fetchTrip();
  }

  protected confirmToDelete(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(),
    });
  }

  protected updateTrip(formValues: TripFormValue): void {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);
    const command: UpdateTripCommand = {
      ...formValues,
      name: formValues.tripName,
      tripId: tripId,
    };

    this.apiService.tripApi.updateTrip(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Trip updated successfully");
        this.fetchTrip();
      }
      this.isLoading.set(false);
    });
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
          tripName: trip.name,
          truckId: trip.truckId,
          initialLoads: trip.loads,
          initialStops: trip.stops,
        });

        this.disabledForEditing.set(trip.status !== TripStatus.Draft);
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
