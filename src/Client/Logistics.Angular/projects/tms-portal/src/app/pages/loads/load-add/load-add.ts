import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, createLoad } from "@logistics/shared/api";
import type { CreateLoadCommand } from "@logistics/shared/api/models";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { ToastService } from "@/core/services";
import { LoadFormComponent, type LoadFormValue } from "@/shared/components";

@Component({
  selector: "app-load-add",
  templateUrl: "./load-add.html",
  imports: [ToastModule, CardModule, ProgressSpinnerModule, LoadFormComponent],
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
      originAddress: formValue.originAddress,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId,
      assignedTruckId: formValue.assignedTruckId ?? undefined,
      customerId: formValue.customer?.id,
    };

    await this.api.invoke(createLoad, { body: command });
    this.isLoading.set(false);
    this.toastService.showSuccess("A new load has been created successfully");
    this.router.navigateByUrl("/loads");
  }
}
