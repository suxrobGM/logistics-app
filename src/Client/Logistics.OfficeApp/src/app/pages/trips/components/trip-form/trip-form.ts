import {CurrencyPipe} from "@angular/common";
import {Component, computed, effect, input, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
import {InputGroupModule} from "primeng/inputgroup";
import {InputGroupAddon} from "primeng/inputgroupaddon";
import {InputNumber} from "primeng/inputnumber";
import {InputTextModule} from "primeng/inputtext";
import {ProgressSpinner} from "primeng/progressspinner";
import {TableModule} from "primeng/table";
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
    InputGroupAddon,
    InputNumber,
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
  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly loads = input<TripLoadDto[]>([]);
  public readonly stops = input<TripStopDto[]>([]);

  public readonly save = output<TripFormValue>();
  public readonly remove = output<void>();

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

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }

    this.save.emit(this.form.getRawValue() as TripFormValue);
  }

  private patch(src: Partial<TripFormValue>): void {
    this.form.patchValue({...src});
  }
}
