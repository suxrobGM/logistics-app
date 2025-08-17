import {Component, inject, signal} from "@angular/core";
import {CardModule} from "primeng/card";
import {ApiService} from "@/core/api";
import {CreateTripCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {TripForm, TripFormValue} from "../components";

@Component({
  selector: "app-trip-add",
  templateUrl: "./trip-add.html",
  imports: [CardModule, TripForm],
})
export class TripAddPage {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);

  protected createTrip(formValue: TripFormValue): void {
    this.isLoading.set(true);

    const command: CreateTripCommand = {
      ...formValue,
    };

    this.apiService.tripApi.createTrip(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Trip created successfully");
      }

      this.isLoading.set(false);
    });
  }
}
