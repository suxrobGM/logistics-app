import {CurrencyPipe} from "@angular/common";
import {Component, computed, effect, inject, input, model, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
import {InputGroupModule} from "primeng/inputgroup";
import {InputTextModule} from "primeng/inputtext";
import {ProgressSpinner} from "primeng/progressspinner";
import {StepperModule} from "primeng/stepper";
import {TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {CreateTripLoadCommand, LoadDto, TripLoadDto, TripStopDto} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {
  DirectionMap,
  FormField,
  LoadStatusTag,
  SearchTruckComponent,
  ValidationSummary,
} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";
import {GeoPoint} from "@/shared/types/mapbox";
import {TripLoadDialog} from "../trip-load-dialog/trip-load-dialog";

export interface TripFormValue {
  name: string;
  plannedStart: Date;
  truckId: string;
  newLoads?: CreateTripLoadCommand[];
  attachLoadIds?: string[];
  detachLoadIds?: string[];
}

@Component({
  selector: "app-trip-form",
  templateUrl: "./trip-form.html",
  imports: [
    ValidationSummary,
    ProgressSpinner,
    FormField,
    SearchTruckComponent,
    ReactiveFormsModule,
    InputGroupModule,
    ButtonModule,
    DirectionMap,
    RouterLink,
    InputTextModule,
    TripLoadDialog,
    DatePicker,
    TableModule,
    LoadStatusTag,
    AddressPipe,
    DistanceUnitPipe,
    CurrencyPipe,
    StepperModule,
    TooltipModule,
  ],
})
export class TripForm {
  private readonly attachLoadIds: string[] = [];
  private readonly detachLoadIds: string[] = [];
  private readonly newLoads: CreateTripLoadCommand[] = [];

  //#region Injected services

  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  //#endregion

  //#region Public properties

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly loads = model<TripLoadDto[]>([]);
  public readonly stops = input<TripStopDto[]>([]);

  public readonly save = output<TripFormValue>();
  public readonly remove = output<void>();

  //#endregion

  //#region UI properties

  protected readonly tripLoadDialogVisible = model(false);

  protected readonly totalDistance = computed(() =>
    this.loads().reduce((total, load) => total + load.distance, 0)
  );
  protected readonly totalCost = computed(() =>
    this.loads().reduce((total, load) => total + load.deliveryCost, 0)
  );
  protected readonly stopCoords = computed(() =>
    this.stops().map((stop) => [stop.addressLong, stop.addressLat] as GeoPoint)
  );

  //#endregion

  //#region Form

  protected readonly form = new FormGroup({
    name: new FormControl<string>("", {validators: [Validators.required], nonNullable: true}),
    plannedStart: new FormControl<Date>(new Date(), {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckId: new FormControl<string>("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
  });

  //#endregion

  constructor() {
    effect(() => {
      const initialData = this.initial();

      if (initialData) {
        this.form.patchValue({...initialData});
      }
    });
  }

  protected attachLoad(load: LoadDto): void {
    this.attachLoadIds.push(load.id);
  }

  protected detachLoad(load: TripLoadDto): void {
    this.detachLoadIds.push(load.id);
  }

  protected addNewLoad(load: CreateTripLoadCommand): void {
    this.newLoads.push(load);
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.save.emit(this.form.getRawValue() satisfies TripFormValue);
  }
}
