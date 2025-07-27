import {CommonModule} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {Router} from "@angular/router";
import {ConfirmationService} from "primeng/api";
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
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly id = input<string>();

  protected readonly isLoading = signal(false);
  protected readonly initialData = signal<Partial<LoadFormValue> | null>(null);
  protected readonly loadNumber = signal<number>(0);

  ngOnInit(): void {
    this.fetchLoad();
  }

  confirmToDelete(): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this load?",
      accept: () => this.deleteLoad(),
    });
  }

  update(formValue: LoadFormValue): void {
    const command: UpdateLoadCommand = {
      id: this.id()!,
      name: formValue.name!,
      loadType: formValue.loadType!,
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
      customerId: formValue.customer?.id,
      status: formValue.status,
    };

    this.apiService.loadApi.updateLoad(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Load has been updated successfully");
      }
    });
  }

  private deleteLoad(): void {
    this.isLoading.set(true);
    this.apiService.loadApi.deleteLoad(this.id()!).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A load has been deleted successfully");
        this.router.navigateByUrl("/loads");
      }

      this.isLoading.set(false);
    });
  }

  private fetchLoad(): void {
    this.isLoading.set(true);

    this.apiService.loadApi.getLoad(this.id()!).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      const load = result.data;

      this.initialData.set({
        name: load.name,
        loadType: load.type,
        customer: load.customer,
        orgAddress: load.originAddress,
        orgCoords: [load.originAddressLong, load.originAddressLat],
        dstAddress: load.destinationAddress,
        dstCoords: [load.destinationAddressLong, load.destinationAddressLat],
        deliveryCost: load.deliveryCost,
        distance: Converters.metersTo(load.distance, "mi"),
        status: load.status,
        assignedDispatcherId: load.assignedDispatcherId,
        assignedDispatcherName: load.assignedDispatcherName,
        assignedTruck: {
          truckId: load.assignedTruckId!,
          driversName: this.formatDriversName(
            load.assignedTruckNumber!,
            load.assignedTruckDriversName!
          ),
        },
      });

      console.log("load", load);

      this.loadNumber.set(load.number);
      this.isLoading.set(false);
    });
  }

  private formatDriversName(truckNumber: string, driversName: string[]): string {
    const formattedDriversName = driversName.join(",");
    return `${truckNumber} - ${formattedDriversName}`;
  }
}
