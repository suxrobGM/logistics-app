import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { Button } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { TruckDto, TruckType } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { FormField, SearchTruckComponent, ValidationSummary } from "@/shared/components";

export interface TripWizardBasicData {
  tripName: string;
  truckId: string;
  truckVehicleCapacity: number;
}

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

  public readonly stepData = input<TripWizardBasicData | null>(null);
  public readonly disabled = input<boolean>(false);
  public readonly next = output<TripWizardBasicData>();

  protected readonly form = new FormGroup({
    tripName: new FormControl<string>("", { validators: [Validators.required], nonNullable: true }),
    truck: new FormControl<TruckDto | string | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
  });

  constructor() {
    effect(() => {
      const stepData = this.stepData();

      // Initialize form from step data
      if (stepData) {
        this.form.patchValue({
          tripName: stepData.tripName ?? "",
          truck: stepData.truckId ?? null,
        });
      }

      // Enable or disable form controls based on the disabled input
      if (this.disabled()) {
        this.form.get("truckId")?.disable();
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

    this.next.emit({
      tripName: this.form.value.tripName ?? this.stepData()?.tripName ?? "",
      truckId: (this.form.value.truck as TruckDto)?.id ?? this.stepData()?.truckId ?? "",
      truckVehicleCapacity:
        (this.form.value.truck as TruckDto)?.vehicleCapacity ??
        this.stepData()?.truckVehicleCapacity ??
        0,
    });
  }
}
