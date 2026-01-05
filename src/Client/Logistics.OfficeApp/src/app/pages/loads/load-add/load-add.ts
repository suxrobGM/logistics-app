import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { Api, createLoad$Json } from "@/core/api";
import { CreateLoadCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { LoadFormComponent, LoadFormValue } from "@/shared/components";

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
      assignedTruckId: formValue.assignedTruckId,
      customerId: formValue.customer?.id,
    };

    const result = await this.api.invoke(createLoad$Json, { body: command });
    if (result.success) {
      this.isLoading.set(false);
      this.toastService.showSuccess("A new load has been created successfully");
      this.router.navigateByUrl("/loads");
    }
  }
}
