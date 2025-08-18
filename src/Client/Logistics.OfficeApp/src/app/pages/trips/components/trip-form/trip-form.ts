import {Component, computed, input, model, output, signal} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {InputGroupModule} from "primeng/inputgroup";
import {InputTextModule} from "primeng/inputtext";
import {ProgressSpinner} from "primeng/progressspinner";
import {StepperModule} from "primeng/stepper";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {CreateTripLoadCommand, TripLoadDto, TripStopDto} from "@/core/api/models";
import {GeoPoint} from "@/shared/types/mapbox";
import {BasicStepData, TripFormStepBasic} from "../trip-form-step-basic/trip-form-step-basic";
import {LoadsStepData, TripFormStepLoads} from "../trip-form-step-loads/trip-form-step-loads";
import {TripFormStepReview} from "../trip-form-step-review/trip-form-step-review";

export interface TripFormValue {
  name: string;
  plannedStart: Date;
  truckId: string;
  newLoads?: CreateTripLoadCommand[];
  attachedLoadIds?: string[];
  detachedLoadIds?: string[];
  initialLoads?: TripLoadDto[];
  initialStops?: TripStopDto[];
}

@Component({
  selector: "app-trip-form",
  templateUrl: "./trip-form.html",
  imports: [
    ProgressSpinner,
    InputGroupModule,
    ButtonModule,
    InputTextModule,
    TableModule,
    StepperModule,
    TooltipModule,
    TagModule,
    TripFormStepReview,
    TripFormStepBasic,
    TripFormStepLoads,
  ],
})
export class TripForm {
  protected readonly formValue = signal<TripFormValue | null>(null);
  protected readonly activeStep = signal(1);

  protected readonly stopCoords = computed(() =>
    this.stops().map((stop) => [stop.addressLong, stop.addressLat] as GeoPoint)
  );

  public readonly mode = input.required<"create" | "edit">();
  public readonly initialData = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly loads = model<TripLoadDto[]>([]);
  public readonly stops = input<TripStopDto[]>([]);

  public readonly save = output<TripFormValue>();

  protected processBasicStep(stepData: BasicStepData): void {
    this.formValue.set({
      name: stepData.name,
      plannedStart: stepData.plannedStart,
      truckId: stepData.truckId,
    });

    this.activeStep.set(2);
  }

  protected processLoadsStep(stepData: LoadsStepData): void {
    this.formValue.update((prev) => ({
      ...prev!,
      newLoads: stepData.newLoads,
      attachedLoadIds: stepData.attachedLoads.map((load) => load.id),
      detachLoadIds: stepData.detachedLoads.map((load) => load.id),
    }));

    this.activeStep.set(3);
  }

  protected processReviewStep(): void {
    const formValue = this.formValue();
    if (!formValue) {
      return;
    }

    this.save.emit(formValue);
  }
}
