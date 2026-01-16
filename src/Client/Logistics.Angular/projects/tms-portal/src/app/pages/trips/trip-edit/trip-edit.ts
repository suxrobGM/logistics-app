import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router } from "@angular/router";
import { CardModule } from "primeng/card";
import { Api, deleteTrip, getTripById, updateTrip } from "@/core/api";
import type { UpdateTripCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { TripWizard, type TripWizardValue } from "../components";

@Component({
  selector: "app-trip-edit",
  templateUrl: "./trip-edit.html",
  imports: [CardModule, TripWizard],
})
export class TripEditPage implements OnInit {
  private readonly toastService = inject(ToastService);
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly tripId = input<string>();
  protected readonly disabledForEditing = signal(false);

  protected readonly tripNumber = signal<number | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly initialData = signal<Partial<TripWizardValue> | null>(null);

  ngOnInit(): void {
    this.fetchTrip();
  }

  protected confirmToDelete(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this trip?",
      accept: () => this.deleteTrip(),
    });
  }

  protected async updateTrip(formValues: TripWizardValue): Promise<void> {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);
    const command: UpdateTripCommand = {
      tripId: tripId,
      name: formValues.tripName,
      truckId: formValues.truckId,
      newLoads: formValues.newLoads,
      // attachedLoadIds: formValue.attachedLoads?.map((l) => l.id),
      detachedLoadIds: formValues.detachedLoads
        ?.map((l) => l.id)
        .filter((id): id is string => id != null),
      optimizedStops: formValues.stops,
      totalDistance: formValues.totalDistance,
    };

    await this.api.invoke(updateTrip, { id: tripId, body: command });
    this.toastService.showSuccess("Trip updated successfully");
    this.fetchTrip();
    this.isLoading.set(false);
  }

  private async fetchTrip(): Promise<void> {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);

    const trip = await this.api.invoke(getTripById, { tripId });
    if (trip) {
      this.initialData.set({
        tripName: trip.name ?? undefined,
        truckId: trip.truckId,
        initialLoads: trip.loads ?? undefined,
        initialStops: trip.stops ?? undefined,
        totalDistance: trip.totalDistance ?? 0,
      });

      this.disabledForEditing.set(trip.status !== "draft");
      this.tripNumber.set(trip.number ?? null);
    }

    this.isLoading.set(false);
  }

  private async deleteTrip(): Promise<void> {
    const tripId = this.tripId();

    if (!tripId) {
      return;
    }

    this.isLoading.set(true);

    await this.api.invoke(deleteTrip, { id: tripId });
    this.toastService.showSuccess("A trip has been deleted successfully");
    this.router.navigateByUrl("/trips");

    this.isLoading.set(false);
  }
}
