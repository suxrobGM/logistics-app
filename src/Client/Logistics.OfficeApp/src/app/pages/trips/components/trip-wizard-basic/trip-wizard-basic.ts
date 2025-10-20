import { Component, effect, inject, input } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { Button } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { TruckDto, TruckType } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { FormField, SearchTruckComponent, ValidationSummary } from "@/shared/components";
import { TripWizardStore } from "../../store/trip-wizard-store";

@Component({
  selector: "app-trip-wizard-basic",
  templateUrl: "./trip-wizard-basic.html",
  imports: [
    ValidationSummary,
    FormField,
    SearchTruckComponent,
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
    truck: new FormControl<TruckDto | string | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
  });

  constructor() {
    // Initialize form from store
    effect(() => {
      const tripName = this.store.tripName();
      const truckId = this.store.truckId();

      if (tripName || truckId) {
        this.form.patchValue({
          tripName: tripName || "",
          truck: truckId || null,
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

    const truckType = (this.form.value.truck as TruckDto)?.type;

    if (truckType !== TruckType.CarHauler) {
      this.toastService.showError("The selected truck is not a car hauler truck.");
      return;
    }

    // Update store with basic info
    this.store.setBasicInfo({
      tripName: this.form.value.tripName ?? "",
      truckId: (this.form.value.truck as TruckDto)?.id ?? "",
      truckVehicleCapacity: (this.form.value.truck as TruckDto)?.vehicleCapacity ?? 0,
    });

    // Navigate to next step
    this.store.nextStep();
  }
}
