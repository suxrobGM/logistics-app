import {CurrencyPipe} from "@angular/common";
import {Component, computed, effect, inject, input, model, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
import {InputGroupModule} from "primeng/inputgroup";
import {InputTextModule} from "primeng/inputtext";
import {ProgressSpinner} from "primeng/progressspinner";
import {TableModule} from "primeng/table";
import {ApiService} from "@/core/api";
import {TripLoadDto, TripStopDto} from "@/core/api/models";
import {
  DirectionMap,
  FormField,
  LoadStatusTag,
  SearchTruckComponent,
  ValidationSummary,
} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";
import {GeoPoint} from "@/shared/types/mapbox";
import {LoadDialog} from "../load-dialog/load-dialog";

export interface TripFormValue {
  name: string;
  plannedStart?: Date;
  truckId: string;
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
    LoadDialog,
    DatePicker,
    TableModule,
    LoadStatusTag,
    AddressPipe,
    DistanceUnitPipe,
    CurrencyPipe,
  ],
})
export class TripForm {
  private readonly confirmationService = inject(ConfirmationService);
  private readonly apiService = inject(ApiService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly loads = model<TripLoadDto[]>([]);
  public readonly stops = input<TripStopDto[]>([]);

  public readonly save = output<TripFormValue>();
  public readonly remove = output<void>();

  protected readonly totalDistance = computed(() =>
    this.loads().reduce((total, load) => total + load.distance, 0)
  );
  protected readonly totalCost = computed(() =>
    this.loads().reduce((total, load) => total + load.deliveryCost, 0)
  );
  protected readonly stopCoords = computed(() =>
    this.stops().map((stop) => [stop.addressLong, stop.addressLat] as GeoPoint)
  );

  protected readonly form = new FormGroup({
    name: new FormControl<string>("", {validators: [Validators.required], nonNullable: true}),
    plannedStart: new FormControl<Date | null>(null),
    truckId: new FormControl<string>("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
  });

  constructor() {
    effect(() => {
      const initialData = this.initial();

      if (initialData) {
        this.patch(initialData);
      }
    });
  }

  protected askRemove(): void {
    this.remove.emit();
  }

  protected askRemoveLoad(loadId: string): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this load?",
      accept: () => this.deleteLoad(loadId),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.save.emit(this.form.getRawValue() as TripFormValue);
  }

  private patch(src: Partial<TripFormValue>): void {
    this.form.patchValue({...src});
  }

  private deleteLoad(loadId: string): void {
    this.apiService.loadApi.deleteLoad(loadId).subscribe((result) => {
      if (result.success) {
        this.loads.update((loads) => loads.filter((load) => load.id !== loadId));
      }
    });
  }
}
