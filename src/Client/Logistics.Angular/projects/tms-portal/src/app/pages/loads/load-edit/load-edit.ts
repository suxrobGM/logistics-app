import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  deleteLoad,
  getLoadById,
  updateLoad,
  type UpdateLoadCommand,
} from "@logistics/shared/api";
import { Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { ToastService } from "@/core/services";
import { LoadForm, type LoadFormValue } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-load-edit",
  templateUrl: "./load-edit.html",
  imports: [
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    ProgressSpinnerModule,
    LoadForm,
    Typography,
  ],
})
export class LoadEditComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly id = input<string>();

  protected readonly isLoading = signal(false);
  protected readonly initialData = signal<Partial<LoadFormValue> | null>(null);
  protected readonly loadNumber = signal<number>(0);

  ngOnInit(): void {
    const loadId = this.id();

    if (loadId) {
      this.fetchLoad(loadId);
    }
  }

  protected async updateLoad(formValue: LoadFormValue): Promise<void> {
    const command: UpdateLoadCommand = {
      id: this.id()!,
      name: formValue.name!,
      type: formValue.type!,
      source: formValue.source!,
      originAddress: formValue.originAddress!,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress!,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost!,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId!,
      assignedTruckId: formValue.assignedTruckId ?? undefined,
      customerId: formValue.customer?.id,
      status: formValue.status!,
      requestedPickupDate: formValue.requestedPickupDate ?? null,
      requestedDeliveryDate: formValue.requestedDeliveryDate ?? null,
      containerId: formValue.containerId ?? null,
      originTerminalId: formValue.originTerminalId ?? null,
      destinationTerminalId: formValue.destinationTerminalId ?? null,
      notes: formValue.notes ?? null,
    };

    await this.api.invoke(updateLoad, { id: this.id()!, body: command });
    this.toastService.showSuccess("Load has been updated successfully");
  }

  protected async deleteLoad(): Promise<void> {
    this.isLoading.set(true);
    await this.api.invoke(deleteLoad, { id: this.id()! });
    this.toastService.showSuccess("A load has been deleted successfully");
    this.router.navigateByUrl("/loads");

    this.isLoading.set(false);
  }

  private async fetchLoad(loadId: string): Promise<void> {
    this.isLoading.set(true);

    const load = await this.api.invoke(getLoadById, { id: loadId });
    if (!load) {
      return;
    }

    this.initialData.set({
      name: load.name ?? undefined,
      type: load.type,
      source: load.source ?? "manual",
      customer: load.customer,
      originAddress: load.originAddress,
      originLocation: load.originLocation,
      destinationAddress: load.destinationAddress,
      destinationLocation: load.destinationLocation,
      deliveryCost: load.deliveryCost,
      distance: Converters.metersTo(load.distance ?? 0, "mi"),
      status: load.status,
      assignedDispatcherId: load.assignedDispatcherId ?? undefined,
      assignedDispatcherName: load.assignedDispatcherName ?? undefined,
      assignedTruckId: load.assignedTruckId ?? undefined,
      requestedPickupDate: load.requestedPickupDate ?? null,
      requestedDeliveryDate: load.requestedDeliveryDate ?? null,
      notes: load.notes ?? null,
      // Pass minimal DTOs for the autocompletes; the form picks up the IDs at submit
      container: load.containerId
        ? {
            id: load.containerId,
            number: load.containerNumber ?? null,
            isoType: load.containerIsoType,
          }
        : null,
      originTerminal: load.originTerminalId
        ? {
            id: load.originTerminalId,
            name: load.originTerminalName ?? null,
            code: load.originTerminalCode ?? null,
            countryCode: null,
            address: { line1: null, city: null, state: null, zipCode: null, country: null },
          }
        : null,
      destinationTerminal: load.destinationTerminalId
        ? {
            id: load.destinationTerminalId,
            name: load.destinationTerminalName ?? null,
            code: load.destinationTerminalCode ?? null,
            countryCode: null,
            address: { line1: null, city: null, state: null, zipCode: null, country: null },
          }
        : null,
    });

    this.loadNumber.set(load.number ?? 0);
    this.isLoading.set(false);
  }
}
