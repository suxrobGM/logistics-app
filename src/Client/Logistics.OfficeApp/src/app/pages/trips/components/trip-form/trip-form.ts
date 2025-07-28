import {Component, input, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {InputGroupModule} from "primeng/inputgroup";
import {InputGroupAddon} from "primeng/inputgroupaddon";
import {InputNumber} from "primeng/inputnumber";
import {ProgressSpinner} from "primeng/progressspinner";
import {TruckDto} from "@/core/api/models";
import {
  DirectionMap,
  FormField,
  SearchTruckComponent,
  ValidationSummary,
} from "@/shared/components";

export interface TripFormValue {
  name: string;
  plannedDate?: Date;
  truck: TruckDto;
  loads: string[];
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
  ],
})
export class TripForm {
  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TripFormValue>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl<string>("", {validators: [Validators.required], nonNullable: true}),
    plannedDate: new FormControl<Date | null>(null),
    truck: new FormControl<TruckDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    loads: new FormControl<string[]>([]),
  });

  protected submit() {
    if (this.form.invalid) {
      return;
    }

    this.save.emit(this.form.getRawValue() as TripFormValue);
  }
}
