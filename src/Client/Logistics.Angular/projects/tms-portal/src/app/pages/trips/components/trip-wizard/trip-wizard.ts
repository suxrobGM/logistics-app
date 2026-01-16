import { Component, effect, inject, input, output } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { InputGroupModule } from "primeng/inputgroup";
import { InputTextModule } from "primeng/inputtext";
import { MessageModule } from "primeng/message";
import { StepperModule } from "primeng/stepper";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import type { CreateTripLoadCommand, TripLoadDto, TripStopDto } from "@/core/api/models";
import { TripWizardStore } from "../../store/trip-wizard-store";
import { TripWizardBasic } from "../trip-wizard-basic/trip-wizard-basic";
import { TripFormStepLoads } from "../trip-wizard-loads/trip-wizard-loads";
import { TripWizardReview } from "../trip-wizard-review/trip-wizard-review";

export interface TripWizardValue {
  tripName: string;
  truckId: string;
  truckVehicleCapacity: number;
  newLoads?: CreateTripLoadCommand[] | null;
  // attachedLoads removed - API doesn't support attaching existing loads
  detachedLoads?: TripLoadDto[] | null;
  stops: TripStopDto[];
  totalDistance: number;
  totalCost: number;
  totalLoads: number;
  initialLoads?: TripLoadDto[];
  initialStops?: TripStopDto[];
}

@Component({
  selector: "app-trip-wizard",
  templateUrl: "./trip-wizard.html",
  providers: [TripWizardStore],
  imports: [
    InputGroupModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    StepperModule,
    TooltipModule,
    TagModule,
    TripWizardReview,
    TripWizardBasic,
    TripFormStepLoads,
    MessageModule,
  ],
})
export class TripWizard {
  protected readonly store = inject(TripWizardStore);

  public readonly mode = input.required<"create" | "edit">();
  public readonly disabled = input<boolean>(false);
  public readonly initialData = input<Partial<TripWizardValue> | null>(null);

  public readonly save = output<TripWizardValue>();

  // Expose store state for template
  protected readonly activeStep = this.store.activeStep;

  constructor() {
    // Initialize store when initialData changes
    effect(() => {
      const mode = this.mode();
      const initialData = this.initialData();

      if (initialData) {
        this.store.initialize({
          mode,
          tripName: initialData.tripName,
          truckId: initialData.truckId,
          truckVehicleCapacity: initialData.truckVehicleCapacity,
          loads: initialData.initialLoads,
          stops: initialData.initialStops,
          totalDistance: initialData.totalDistance,
          totalCost: initialData.totalCost,
        });
      } else {
        this.store.initialize({ mode });
      }
    });
  }

  protected processStep3(): void {
    this.save.emit(this.store.wizardValue());
  }
}
