import {Component, effect, input, output} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {Button} from "primeng/button";
import {InputTextModule} from "primeng/inputtext";
import {TripDto, TruckDto} from "@/core/api/models";
import {FormField, SearchTruckComponent, ValidationSummary} from "@/shared/components";

export interface BasicStepData {
  name: string;
  truckId: string;
}

@Component({
  selector: "app-trip-form-step-basic",
  templateUrl: "./trip-form-step-basic.html",
  imports: [
    ValidationSummary,
    FormField,
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
    truck: new FormControl<TruckDto | string | null>(null, {
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

      if (this.disabled()) {
        this.form.get("truckId")?.disable();
      } else {
        this.form.enable();
      }
    });
  }

  protected goToNextStep(): void {
    if (this.form.valid) {
      this.next.emit({
        name: this.form.value.name ?? this.initialData()?.name ?? "",
        truckId: (this.form.value.truck as TruckDto)?.id ?? this.initialData()?.truckId ?? "",
      });
    }
  }
}
