import { Component, effect, inject, input, signal } from "@angular/core";
import { disabled, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import type { TruckDto } from "@logistics/shared/api";
import { Stack, UiButton, UiTextField } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { SearchTruck, UiFormField, ValidatedForm } from "@/shared/components";
import { TripWizardStore } from "../../store/trip-wizard-store";

interface TripBasicModel {
  tripName: string;
  // Truck assignment is optional - trip can be created without truck (e.g., from load board).
  truck: TruckDto | null;
}

@Component({
  selector: "app-trip-wizard-basic",
  templateUrl: "./trip-wizard-basic.html",
  imports: [
    FormField,
    FormRoot,
    RouterLink,
    SearchTruck,
    Stack,
    UiButton,
    UiFormField,
    UiTextField,
    ValidatedForm,
  ],
})
export class TripWizardBasic {
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(TripWizardStore);

  public readonly disabled = input<boolean>(false);

  protected readonly model = signal<TripBasicModel>({ tripName: "", truck: null });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.tripName, { message: "Trip name is required." });
      disabled(p.truck, { when: () => this.disabled() });
    },
    {
      submission: {
        action: async () => {
          // A disabled truck is treated as "no truck" in the payload.
          const truck = this.form.truck().disabled() ? null : this.model().truck;

          // Only validate truck type if a truck is selected.
          if (truck && truck.type !== "car_hauler") {
            this.toastService.showError("The selected truck is not a car hauler truck.");
            return undefined;
          }

          // Update store with basic info.
          this.store.setBasicInfo({
            tripName: this.model().tripName,
            truckId: truck?.id ?? null,
            truckNumber: truck?.number ?? null,
            truckVehicleCapacity: truck?.vehicleCapacity ?? 0,
          });

          // Navigate to next step.
          this.store.nextStep();
          return undefined;
        },
      },
    },
  );

  constructor() {
    // Seed the form from the store (e.g. when editing an existing trip). The truck field is seeded
    // with the bare truck id; SearchTruck resolves it to the full TruckDto at runtime.
    effect(() => {
      const tripName = this.store.tripName();
      const truckId = this.store.truckId();

      if (tripName || truckId) {
        this.model.update((v) => ({
          ...v,
          tripName,
          truck: (truckId ?? null) as unknown as TruckDto | null,
        }));
      }
    });
  }
}
