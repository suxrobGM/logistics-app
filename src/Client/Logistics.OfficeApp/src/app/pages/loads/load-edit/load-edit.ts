import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router } from "@angular/router";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { Api, deleteLoad, getLoadById$Json, updateLoad } from "@/core/api";
import type { UpdateLoadCommand } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { LoadFormComponent, type LoadFormValue } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-load-edit",
  templateUrl: "./load-edit.html",
  imports: [ToastModule, ConfirmDialogModule, CardModule, ProgressSpinnerModule, LoadFormComponent],
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
      originAddress: formValue.originAddress!,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress!,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost!,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId!,
      assignedTruckId: formValue.assignedTruckId!,
      customerId: formValue.customer?.id,
      status: formValue.status!,
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

    const load = await this.api.invoke(getLoadById$Json, { id: loadId });
    if (!load) {
      return;
    }

    this.initialData.set({
      name: load.name ?? undefined,
      type: load.type,
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
    });

    this.loadNumber.set(load.number ?? 0);
    this.isLoading.set(false);
  }
}
