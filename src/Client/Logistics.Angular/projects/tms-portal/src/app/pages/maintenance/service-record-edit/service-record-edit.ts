import { Component, inject, input, type OnInit, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, getMaintenanceRecords } from "@logistics/shared/api";
import type { MaintenanceRecordDto, TruckDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import {
  MaintenanceRecordForm,
  type MaintenanceRecordFormValue,
} from "../components/maintenance-record-form/maintenance-record-form";

@Component({
  selector: "app-service-record-edit",
  templateUrl: "./service-record-edit.html",
  imports: [CardModule, ProgressSpinnerModule, PageHeader, MaintenanceRecordForm],
})
export class ServiceRecordEditPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly record = signal<MaintenanceRecordDto | null>(null);
  protected readonly initialFormValue = signal<Partial<MaintenanceRecordFormValue> | null>(null);

  async ngOnInit(): Promise<void> {
    await this.loadRecord();
  }

  private async loadRecord(): Promise<void> {
    this.isLoading.set(true);
    try {
      // TODO: Use getMaintenanceRecordById once API is regenerated
      // For now, fetch from list and filter
      const result = await this.api.invoke(getMaintenanceRecords, {});
      const found = result?.items?.find((r: MaintenanceRecordDto) => r.id === this.id());

      if (found) {
        this.record.set(found);

        // Map to form value
        const formValue: Partial<MaintenanceRecordFormValue> = {
          truck: {
            id: found.truckId,
            truckNumber: found.truckNumber,
          } as TruckDto,
          type: found.type,
          description: found.description ?? "",
          serviceDate: found.serviceDate ? new Date(found.serviceDate) : new Date(),
          odometerReading: found.odometerReading,
          engineHours: found.engineHours,
          vendorName: found.vendorName,
          invoiceNumber: found.invoiceNumber,
          laborCost: found.laborCost ?? 0,
          partsCost: found.partsCost ?? 0,
          notes: found.notes,
        };

        this.initialFormValue.set(formValue);
      } else {
        this.toastService.showError("Maintenance record not found");
        this.router.navigateByUrl("/maintenance/records");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onSave(record: MaintenanceRecordDto): void {
    this.router.navigateByUrl(`/maintenance/records/${record.id}`);
  }
}
