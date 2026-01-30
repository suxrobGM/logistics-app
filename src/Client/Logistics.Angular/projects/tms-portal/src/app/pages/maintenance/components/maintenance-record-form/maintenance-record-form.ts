import { Component, effect, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { Api, createMaintenanceRecord } from "@logistics/shared/api";
import type {
  CreateMaintenanceRecordCommand,
  MaintenanceRecordDto,
  MaintenanceType,
  TruckDto,
} from "@logistics/shared/api";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DatePickerModule } from "primeng/datepicker";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { SearchTruck } from "@/shared/components/search";

export interface MaintenanceRecordFormValue {
  truck: TruckDto | null;
  type: MaintenanceType;
  description: string;
  serviceDate: Date;
  odometerReading: number | null;
  engineHours: number | null;
  vendorName: string | null;
  invoiceNumber: string | null;
  laborCost: number;
  partsCost: number;
  notes: string | null;
}

const maintenanceTypeOptions = [
  { label: "Oil Change", value: "oil_change" },
  { label: "Tire Rotation", value: "tire_rotation" },
  { label: "Tire Replacement", value: "tire_replacement" },
  { label: "Brake Inspection", value: "brake_inspection" },
  { label: "Brake Replacement", value: "brake_replacement" },
  { label: "Air Filter Replacement", value: "air_filter_replacement" },
  { label: "Fuel Filter Replacement", value: "fuel_filter_replacement" },
  { label: "Transmission Service", value: "transmission_service" },
  { label: "Coolant Flush", value: "coolant_flush" },
  { label: "Belt Inspection", value: "belt_inspection" },
  { label: "Battery", value: "battery" },
  { label: "Annual DOT Inspection", value: "annual_dot_inspection" },
  { label: "Preventive Maintenance", value: "preventive_maintenance" },
  { label: "Engine Service", value: "engine_service" },
  { label: "Suspension Service", value: "suspension_service" },
  { label: "Electrical Repair", value: "electrical_repair" },
  { label: "Body Work", value: "body_work" },
  { label: "HVAC Service", value: "hvac_service" },
  { label: "Exhaust System", value: "exhaust_system" },
  { label: "Steering Repair", value: "steering_repair" },
  { label: "Other", value: "other" },
];

@Component({
  selector: "app-maintenance-record-form",
  templateUrl: "./maintenance-record-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    DatePickerModule,
    SearchTruck,
  ],
})
export class MaintenanceRecordForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly maintenanceTypeOptions = maintenanceTypeOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>();
  public readonly initial = input<Partial<MaintenanceRecordFormValue> | null>(null);

  public readonly save = output<MaintenanceRecordDto>();

  protected readonly form = new FormGroup({
    truck: new FormControl<TruckDto | null>(null, { validators: Validators.required }),
    type: new FormControl<MaintenanceType>("oil_change", {
      validators: Validators.required,
      nonNullable: true,
    }),
    description: new FormControl("", { validators: Validators.required, nonNullable: true }),
    serviceDate: new FormControl<Date>(new Date(), {
      validators: Validators.required,
      nonNullable: true,
    }),
    odometerReading: new FormControl<number | null>(null),
    engineHours: new FormControl<number | null>(null),
    vendorName: new FormControl<string | null>(null),
    invoiceNumber: new FormControl<string | null>(null),
    laborCost: new FormControl<number>(0, { nonNullable: true }),
    partsCost: new FormControl<number>(0, { nonNullable: true }),
    notes: new FormControl<string | null>(null),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const formValue = this.form.getRawValue();

    try {
      if (this.mode() === "create") {
        const command: CreateMaintenanceRecordCommand = {
          truckId: formValue.truck!.id!,
          type: formValue.type,
          description: formValue.description,
          serviceDate: formValue.serviceDate.toISOString(),
          odometerReading: formValue.odometerReading,
          engineHours: formValue.engineHours,
          vendorName: formValue.vendorName,
          invoiceNumber: formValue.invoiceNumber,
          laborCost: formValue.laborCost,
          partsCost: formValue.partsCost,
          notes: formValue.notes,
        };

        const result = await this.api.invoke(createMaintenanceRecord, { body: command });
        if (result) {
          this.toastService.showSuccess("Maintenance record created successfully");
          this.save.emit(result);
        }
      } else {
        // Update functionality will be added when API is regenerated
        this.toastService.showWarning("Update functionality pending API regeneration");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  private patch(src: Partial<MaintenanceRecordFormValue>): void {
    this.form.patchValue({
      ...src,
      serviceDate: src.serviceDate ? new Date(src.serviceDate) : new Date(),
    });
  }
}
