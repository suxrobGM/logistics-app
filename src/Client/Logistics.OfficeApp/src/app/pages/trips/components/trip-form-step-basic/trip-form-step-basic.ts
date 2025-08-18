import {Component, effect, input, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
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
  ],
})
export class TripFormStepBasic {
  public readonly initialData = input<Partial<BasicStepData> | null>(null);
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
        this.form.patchValue(initialData);
      }
    });
  }

  protected goToNextStep(): void {
    if (this.form.valid) {
      this.next.emit(this.form.value as BasicStepData);
    }
  }
}
