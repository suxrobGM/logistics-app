import {Component, inject, model, signal} from "@angular/core";
import {DialogModule} from "primeng/dialog";
import {ApiService} from "@/core/api";
import {CreateLoadCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {LoadFormComponent, LoadFormValue} from "@/shared/components";

@Component({
  selector: "app-add-load-dialog",
  imports: [DialogModule, LoadFormComponent],
  templateUrl: "./add-load-dialog.html",
})
export class AddLoadDialog {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  public readonly visible = model(false);
  protected readonly isLoading = signal(false);

  create(formValue: LoadFormValue): void {
    this.isLoading.set(true);
    const command: CreateLoadCommand = {
      name: formValue.name,
      loadType: formValue.loadType,
      originAddress: formValue.orgAddress!,
      originAddressLong: formValue.orgCoords![0],
      originAddressLat: formValue.orgCoords![1],
      destinationAddress: formValue.dstAddress!,
      destinationAddressLong: formValue.dstCoords![0],
      destinationAddressLat: formValue.dstCoords![1],
      deliveryCost: formValue.deliveryCost!,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId!,
      assignedTruckId: formValue.assignedTruck!.truckId,
      customerId: formValue.customer!.id,
    };

    this.apiService.loadApi.createLoad(command).subscribe((result) => {
      if (result.success) {
        this.isLoading.set(false);
        this.toastService.showSuccess("A new load has been created successfully");
      }
    });
  }
}
