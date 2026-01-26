import { Component, effect, inject, input } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import type { TruckDto } from "@logistics/shared/api";
import { Button } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { ToastService } from "@/core/services";
import { LabeledField, SearchTruck, ValidationSummary } from "@/shared/components";
import { TripWizardStore } from "../../store/trip-wizard-store";

@Component({
  selector: "app-trip-wizard-basic",
  templateUrl: "./trip-wizard-basic.html",
  imports: [
    ValidationSummary,
    LabeledField,
    SearchTruck,
    Button,
    RouterLink,
    ReactiveFormsModule,
    InputTextModule,
  ],
})
export class TripWizardBasic {
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(TripWizardStore);

  public readonly disabled = input<boolean>(false);

  protected readonly form = new FormGroup({
    tripName: new FormControl<string>("", { validators: [Validators.required], nonNullable: true }),
    // Truck assignment is optional - trip can be created without truck (e.g., from load board)
    truck: new FormControl<TruckDto | string | null>(null, { nonNullable: true }),
  });

  constructor() {
    // Initialize form from store
    effect(() => {
      const tripName = this.store.tripName();
      const truckId = this.store.truckId();

      if (tripName || truckId) {
        this.form.patchValue({
          tripName: tripName,
          truck: truckId,
        });
      }

      // Enable or disable form controls based on the disabled input
      if (this.disabled()) {
        this.form.get("truck")?.disable();
      } else {
        this.form.enable();
      }
    });
  }

  protected goToNextStep(): void {
    if (!this.form.valid) {
      return;
    }

    const truck = this.form.value.truck as TruckDto | null;

    // Only validate truck type if a truck is selected
    if (truck && truck.type !== "car_hauler") {
      this.toastService.showError("The selected truck is not a car hauler truck.");
      return;
    }

    // Update store with basic info
    this.store.setBasicInfo({
      tripName: this.form.value.tripName ?? "",
      truckId: truck?.id ?? null,
      truckVehicleCapacity: truck?.vehicleCapacity ?? 0,
    });

    // Navigate to next step
    this.store.nextStep();
  }
}
