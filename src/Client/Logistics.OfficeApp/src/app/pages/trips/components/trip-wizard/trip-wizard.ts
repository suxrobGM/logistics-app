import {Component, effect, input, output, signal} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {InputGroupModule} from "primeng/inputgroup";
import {InputTextModule} from "primeng/inputtext";
import {MessageModule} from "primeng/message";
import {StepperModule} from "primeng/stepper";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {TripLoadDto, TripStopDto} from "@/core/api/models";
import {TripWizardBasic, TripWizardBasicData} from "../trip-wizard-basic/trip-wizard-basic";
import {TripFormStepLoads, TripWizardLoadsData} from "../trip-wizard-loads/trip-wizard-loads";
import {TripWizardReview, TripWizardReviewData} from "../trip-wizard-review/trip-wizard-review";

export interface TripWizardValue extends TripWizardBasicData, TripWizardLoadsData {
  initialLoads?: TripLoadDto[];
  initialStops?: TripStopDto[];
}

@Component({
  selector: "app-trip-wizard",
  templateUrl: "./trip-wizard.html",
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
  protected readonly step1Data = signal<TripWizardBasicData | null>(null);
  protected readonly step2Data = signal<TripWizardLoadsData | null>(null);
  protected readonly step3Data = signal<TripWizardReviewData | null>(null);
  protected readonly activeStep = signal(1);

  public readonly mode = input.required<"create" | "edit">();
  public readonly disabled = input<boolean>(false);
  public readonly initialData = input<Partial<TripWizardValue> | null>(null);

  public readonly save = output<TripWizardValue>();

  constructor() {
    // Initialize step data
    effect(() => {
      const initialData = this.initialData();

      if (initialData) {
        this.step1Data.set({
          tripName: initialData.tripName ?? "",
          truckId: initialData.truckId ?? "",
          truckVehicleCapacity: initialData.truckVehicleCapacity ?? 0,
        });

        this.step2Data.set({
          newLoads: initialData.newLoads ?? [],
          attachedLoads: initialData.initialLoads ?? [],
          detachedLoads: initialData.detachedLoads ?? [],
          stops: initialData.initialStops ?? [],
          totalDistance: initialData.totalDistance ?? 0,
          totalCost: initialData.totalCost ?? 0,
          totalLoads: initialData.totalLoads ?? 0,
          truckId: initialData.truckId ?? "",
          truckVehicleCapacity: initialData.truckVehicleCapacity ?? 0,
        });
      }
    });
  }

  protected processStep1(stepData: TripWizardBasicData): void {
    this.step1Data.set(stepData);
    this.step2Data.update((data) => ({
      ...data!,
      truckId: stepData.truckId,
      truckVehicleCapacity: stepData.truckVehicleCapacity,
    }));
    this.activeStep.set(2);
  }

  protected processStep2(stepData: TripWizardLoadsData): void {
    // Set data for the last review step
    this.step2Data.set(stepData);
    this.step3Data.set({
      ...this.step1Data()!,
      ...stepData,
      newLoadsCount: stepData.newLoads?.length ?? 0,
      pendingDetachLoadsCount: stepData.detachedLoads?.length ?? 0,
      truckVehicleCapacity: stepData.truckVehicleCapacity,
    });
    this.activeStep.set(3);
  }

  protected processStep3(): void {
    // All steps data except for step 3, since step 3 is review step so do not need to include it
    const allStepsData = {
      ...this.step1Data()!,
      ...this.step2Data()!,
    };

    this.save.emit(allStepsData);
  }
}
