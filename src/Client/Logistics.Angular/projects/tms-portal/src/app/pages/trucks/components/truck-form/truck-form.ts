import { Component, type OnInit, computed, inject, input, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { RouterLink } from "@angular/router";
import { Api, getEmployees } from "@logistics/shared/api";
import type { EmployeeDto, TruckDto, TruckStatus, TruckType } from "@logistics/shared/api";
import { truckStatusOptions, truckTypeOptions } from "@logistics/shared/api/enums";
import { AutoCompleteModule } from "primeng/autocomplete";
import { ButtonModule } from "primeng/button";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { ToastService } from "@/core/services";
import { LabeledField, ValidationSummary } from "@/shared/components";

export interface TruckFormData {
  truckNumber: string;
  truckType: TruckType;
  truckStatus: TruckStatus;
  mainDriver: EmployeeDto | null;
  secondaryDriver: EmployeeDto | null;
  vehicleCapacity: number | null;
  make: string | null;
  model: string | null;
  year: number | null;
  vin: string | null;
  licensePlate: string | null;
  licensePlateState: string | null;
}

@Component({
  selector: "app-truck-form",
  templateUrl: "./truck-form.html",
  imports: [
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    AutoCompleteModule,
    LabeledField,
    ValidationSummary,
  ],
})
export class TruckForm implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<TruckDto | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TruckFormData>();
  public readonly remove = output<void>();

  protected readonly truckTypes = truckTypeOptions;
  protected readonly truckStatuses = truckStatusOptions;
  protected readonly suggestedDrivers = signal<EmployeeDto[]>([]);

  protected readonly form = new FormGroup({
    truckNumber: new FormControl<string>("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckType: new FormControl<TruckType>("freight_truck", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckStatus: new FormControl<TruckStatus>("available", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    mainDriver: new FormControl<EmployeeDto | null>(null),
    secondaryDriver: new FormControl<EmployeeDto | null>(null),
    vehicleCapacity: new FormControl<number | null>(null),
    make: new FormControl<string | null>(null),
    model: new FormControl<string | null>(null),
    year: new FormControl<number | null>(null),
    vin: new FormControl<string | null>(null),
    licensePlate: new FormControl<string | null>(null),
    licensePlateState: new FormControl<string | null>(null),
  });

  protected readonly isCarHauler = computed(() => {
    return this.form.get("truckType")?.value === "car_hauler";
  });

  ngOnInit(): void {
    const initial = this.initial();
    if (initial) {
      this.form.patchValue({
        truckNumber: initial.number ?? "",
        truckType: initial.type ?? "freight_truck",
        truckStatus: initial.status ?? "available",
        mainDriver: initial.mainDriver ?? null,
        secondaryDriver: initial.secondaryDriver ?? null,
        vehicleCapacity: initial.vehicleCapacity ?? null,
        make: initial.make ?? null,
        model: initial.model ?? null,
        year: initial.year ?? null,
        vin: initial.vin ?? null,
        licensePlate: initial.licensePlate ?? null,
        licensePlateState: initial.licensePlateState ?? null,
      });
    }

    // Watch for truck type changes to show/hide vehicle capacity
    this.form.get("truckType")?.valueChanges.subscribe((type) => {
      if (type !== "car_hauler") {
        this.form.patchValue({ vehicleCapacity: null });
      }
    });
  }

  protected async searchDriver(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getEmployees, {
      Search: event.query,
      Role: "Driver",
    });
    if (result.items) {
      this.suggestedDrivers.set(result.items);
    }
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit({
      truckNumber: this.form.value.truckNumber!,
      truckType: this.form.value.truckType!,
      truckStatus: this.form.value.truckStatus!,
      mainDriver: this.form.value.mainDriver ?? null,
      secondaryDriver: this.form.value.secondaryDriver ?? null,
      vehicleCapacity: this.form.value.vehicleCapacity ?? null,
      make: this.form.value.make ?? null,
      model: this.form.value.model ?? null,
      year: this.form.value.year ?? null,
      vin: this.form.value.vin ?? null,
      licensePlate: this.form.value.licensePlate ?? null,
      licensePlateState: this.form.value.licensePlateState ?? null,
    });
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure you want to delete this truck?",
      accept: () => this.remove.emit(),
    });
  }
}
