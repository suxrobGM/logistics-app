import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { CardModule } from "primeng/card";
import { Api, createTrip } from "@/core/api";
import type { CreateTripCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { TripWizard, type TripWizardValue } from "../components";

@Component({
  selector: "app-trip-add",
  templateUrl: "./trip-add.html",
  imports: [CardModule, TripWizard],
})
export class TripAddPage {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);

  protected async createTrip(formValue: TripWizardValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateTripCommand = {
      name: formValue.tripName,
      truckId: formValue.truckId,
      newLoads: formValue.newLoads,
      // attachedLoadIds: formValue.attachedLoads?.map((l) => l.id),
      optimizedStops: formValue.stops,
      totalDistance: formValue.totalDistance,
    };

    await this.api.invoke(createTrip, { body: command });
    this.toastService.showSuccess("Trip created successfully");
    this.router.navigate(["/trips"]);

    this.isLoading.set(false);
  }
}
