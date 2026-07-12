import { Component, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import {
  Api,
  createMaintenanceRecord,
  updateMaintenanceRecord,
  type CreateMaintenanceRecordCommand,
  type MaintenanceRecordDto,
  type MaintenanceType,
  type TruckDto,
  type UpdateMaintenanceRecordCommand,
} from "@logistics/shared/api";
import {
  Spinner,
  UiButton,
  UiDateField,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { SearchTruck } from "@/shared/components/search";

export interface MaintenanceRecordFormValue {
  truck: TruckDto | null;
  type: MaintenanceType;
  description: string;
  serviceDate: Date;
  odometerReading: number | null;
  engineHours: number | null;
  vendorName: string;
  invoiceNumber: string;
  laborCost: number;
  partsCost: number;
  notes: string;
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
    FormField,
    FormRoot,
    RouterLink,
    SearchTruck,
    Spinner,
    UiButton,
    UiDateField,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class MaintenanceRecordForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly maintenanceTypeOptions = maintenanceTypeOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly id = input<string>();
  public readonly initial = input<Partial<MaintenanceRecordFormValue> | null>(null);

  public readonly save = output<MaintenanceRecordDto>();

  protected readonly model = signal<MaintenanceRecordFormValue>({
    truck: null,
    type: "oil_change",
    description: "",
    serviceDate: new Date(),
    odometerReading: null,
    engineHours: null,
    vendorName: "",
    invoiceNumber: "",
    laborCost: 0,
    partsCost: 0,
    notes: "",
  });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.truck, { message: "Truck is required." });
      required(p.type, { message: "Service type is required." });
      required(p.description, { message: "Description is required." });
      required(p.serviceDate, { message: "Service date is required." });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();

          if (this.mode() === "create") {
            const command: CreateMaintenanceRecordCommand = {
              truckId: value.truck!.id!,
              type: value.type,
              description: value.description,
              serviceDate: value.serviceDate.toISOString(),
              odometerReading: value.odometerReading,
              engineHours: value.engineHours,
              vendorName: value.vendorName || null,
              invoiceNumber: value.invoiceNumber || null,
              laborCost: value.laborCost,
              partsCost: value.partsCost,
              notes: value.notes || null,
            };

            const result = await this.api.invoke(createMaintenanceRecord, { body: command });
            if (result) {
              this.toastService.showSuccess("Maintenance record created successfully");
              this.save.emit(result);
            }
          } else {
            const command: UpdateMaintenanceRecordCommand = {
              id: this.id()!,
              truckId: value.truck!.id!,
              type: value.type,
              description: value.description,
              serviceDate: value.serviceDate.toISOString(),
              odometerReading: value.odometerReading,
              engineHours: value.engineHours,
              vendorName: value.vendorName || null,
              invoiceNumber: value.invoiceNumber || null,
              laborCost: value.laborCost,
              partsCost: value.partsCost,
              notes: value.notes || null,
            };

            const result = await this.api.invoke(updateMaintenanceRecord, {
              id: this.id()!,
              body: command,
            });
            if (result) {
              this.toastService.showSuccess("Maintenance record updated successfully");
              this.save.emit(result);
            }
          }

          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  private patch(src: Partial<MaintenanceRecordFormValue>): void {
    this.model.update((v) => ({
      ...v,
      ...src,
      serviceDate: src.serviceDate ? new Date(src.serviceDate) : new Date(),
    }));
  }
}
