import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { CardModule } from "primeng/card";
import { ApiService } from "@/core/api";
import { CreateTripCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { TripWizard, TripWizardValue } from "../components";

@Component({
  selector: "app-trip-add",
  templateUrl: "./trip-add.html",
  imports: [CardModule, TripWizard],
})
export class TripAddPage {
  private readonly router = inject(Router);
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);

  protected createTrip(formValue: TripWizardValue): void {
    this.isLoading.set(true);

    const command: CreateTripCommand = {
      name: formValue.tripName,
      truckId: formValue.truckId,
      newLoads: formValue.newLoads,
      // attachedLoadIds: formValue.attachedLoads?.map((l) => l.id),
      optimizedStops: formValue.stops,
    };

    this.apiService.tripApi.createTrip(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Trip created successfully");
        this.router.navigate(["/trips"]);
      }

      this.isLoading.set(false);
    });
  }
}
