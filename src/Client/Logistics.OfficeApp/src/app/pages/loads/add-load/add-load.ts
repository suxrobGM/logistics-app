import {Component, inject, signal} from "@angular/core";
import {Router} from "@angular/router";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {CreateLoadCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {LoadFormComponent, LoadFormValue} from "@/shared/components";

@Component({
  selector: "app-add-load",
  templateUrl: "./add-load.html",
  imports: [ToastModule, CardModule, ProgressSpinnerModule, LoadFormComponent],
})
export class AddLoadComponent {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal(false);

  protected create(formValue: LoadFormValue): void {
    this.isLoading.set(true);

    const command: CreateLoadCommand = {
      name: formValue.name,
      loadType: formValue.loadType,
      originAddress: formValue.originAddress,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId,
      assignedTruckId: formValue.assignedTruckId,
      customerId: formValue.customer.id,
    };

    this.apiService.loadApi.createLoad(command).subscribe((result) => {
      if (result.success) {
        this.isLoading.set(false);
        this.toastService.showSuccess("A new load has been created successfully");
        this.router.navigateByUrl("/loads");
      }
    });
  }
}
