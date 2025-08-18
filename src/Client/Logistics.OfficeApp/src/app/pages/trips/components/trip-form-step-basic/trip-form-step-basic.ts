import {Component, effect, input, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
import {InputTextModule} from "primeng/inputtext";
import {TripDto} from "@/core/api/models";
import {FormField, SearchTruckComponent, ValidationSummary} from "@/shared/components";

export interface BasicStepData {
  name: string;
  plannedStart: Date;
  truckId: string;
}

@Component({
  selector: "app-trip-form-step-basic",
  templateUrl: "./trip-form-step-basic.html",
  imports: [
    ValidationSummary,
    FormField,
    DatePicker,
    SearchTruckComponent,
    Button,
    RouterLink,
    ReactiveFormsModule,
    InputTextModule,
  ],
})
export class TripFormStepBasic {
  public readonly initialData = input<Partial<TripDto> | null>(null);
  public readonly disabled = input<boolean>(false);
  public readonly next = output<BasicStepData>();

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

  constructor() {
    effect(() => {
      const initialData = this.initialData();

      if (initialData) {
        console.log("Initial data:", initialData);

        this.form.patchValue(initialData);
      }

      if (this.disabled()) {
        this.form.get("plannedStart")?.disable();
        this.form.get("truckId")?.disable();
      } else {
        this.form.enable();
      }
    });
  }

  protected goToNextStep(): void {
    console.log("form valid:", this.form.valid);

    if (this.form.valid) {
      this.next.emit({
        name: this.form.value.name ?? this.initialData()?.name ?? "",
        plannedStart:
          this.form.value.plannedStart ?? this.initialData()?.plannedStart ?? new Date(),
        truckId: this.form.value.truckId ?? this.initialData()?.truckId ?? "",
      });
    }
  }
}
