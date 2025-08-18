import {CommonModule} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {Router} from "@angular/router";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {UpdateLoadCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {LoadFormComponent, LoadFormValue} from "@/shared/components";
import {Converters} from "@/shared/utils";

@Component({
  selector: "app-edit-load",
  templateUrl: "./edit-load.html",
  imports: [
    CommonModule,
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    ProgressSpinnerModule,
    LoadFormComponent,
  ],
})
export class EditLoadComponent implements OnInit {
  private readonly apiService = inject(ApiService);
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

  protected updateLoad(formValue: LoadFormValue): void {
    const command: UpdateLoadCommand = {
      id: this.id()!,
      name: formValue.name!,
      loadType: formValue.type!,
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

    this.apiService.loadApi.updateLoad(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Load has been updated successfully");
      }
    });
  }

  protected deleteLoad(): void {
    this.isLoading.set(true);
    this.apiService.loadApi.deleteLoad(this.id()!).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A load has been deleted successfully");
        this.router.navigateByUrl("/loads");
      }

      this.isLoading.set(false);
    });
  }

  private fetchLoad(loadId: string): void {
    this.isLoading.set(true);

    this.apiService.loadApi.getLoad(loadId).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      const load = result.data;

      this.initialData.set({
        name: load.name,
        type: load.type,
        customer: load.customer,
        originAddress: load.originAddress,
        originLocation: load.originLocation,
        destinationAddress: load.destinationAddress,
        destinationLocation: load.destinationLocation,
        deliveryCost: load.deliveryCost,
        distance: Converters.metersTo(load.distance, "mi"),
        status: load.status,
        assignedDispatcherId: load.assignedDispatcherId,
        assignedDispatcherName: load.assignedDispatcherName,
        assignedTruckId: load.assignedTruckId,
      });

      this.loadNumber.set(load.number);
      this.isLoading.set(false);
    });
  }
}
