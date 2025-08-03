import {Component, computed, inject, input, model, signal} from "@angular/core";
import {DialogModule} from "primeng/dialog";
import {ApiService} from "@/core/api";
import {CreateLoadCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {LoadFormComponent, LoadFormValue} from "@/shared/components";

@Component({
  selector: "app-load-dialog",
  templateUrl: "./load-dialog.html",
  imports: [DialogModule, LoadFormComponent],
})
export class LoadDialog {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  public readonly visible = model(false);
  public readonly assignedTruckId = input<string>();
  public readonly tripId = input<string>();

  protected readonly isLoading = signal(false);
  protected readonly initialLoad = computed(() => ({
    assignedTruckId: this.assignedTruckId(),
    tripId: this.tripId(),
  }));

  protected create(formValue: LoadFormValue): void {
    this.isLoading.set(true);

    const command: CreateLoadCommand = {
      name: formValue.name,
      loadType: formValue.loadType,
      originAddress: formValue.originAddress!,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress!,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost!,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId!,
      assignedTruckId: formValue.assignedTruckId!,
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
