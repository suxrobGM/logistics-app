import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, createLoad, type CreateLoadCommand } from "@logistics/shared/api";
import { Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { ToastService } from "@/core/services";
import { LoadForm, type LoadFormValue } from "@/shared/components";

@Component({
  selector: "app-load-add",
  templateUrl: "./load-add.html",
  imports: [ToastModule, CardModule, ProgressSpinnerModule, LoadForm, Typography],
})
export class LoadAddComponent {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal(false);

  protected async create(formValue: LoadFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateLoadCommand = {
      name: formValue.name,
      type: formValue.type,
      source: formValue.source,
      originAddress: formValue.originAddress,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId,
      assignedTruckId: formValue.assignedTruckId ?? undefined,
      customerId: formValue.customer?.id,
      requestedPickupDate: formValue.requestedPickupDate ?? null,
      requestedDeliveryDate: formValue.requestedDeliveryDate ?? null,
      containerId: formValue.containerId ?? null,
      originTerminalId: formValue.originTerminalId ?? null,
      destinationTerminalId: formValue.destinationTerminalId ?? null,
      notes: formValue.notes ?? null,
    };

    await this.api.invoke(createLoad, { body: command });
    this.isLoading.set(false);
    this.toastService.showSuccess("A new load has been created successfully");
    this.router.navigateByUrl("/loads");
  }
}
