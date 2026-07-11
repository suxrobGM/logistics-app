import { Component, inject, input, output } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { TripStatus } from "@logistics/shared/api";
import { UiButton } from "@logistics/shared/ui";

@Component({
  selector: "app-trip-actions",
  templateUrl: "./trip-actions.html",
  imports: [RouterLink, UiButton],
})
export class TripActions {
  private readonly toastService = inject(ToastService);

  // Inputs
  public readonly status = input<TripStatus>();
  public readonly truckId = input<string | null>();
  public readonly tripId = input<string>();
  public readonly isProcessing = input<boolean>(false);

  // Outputs
  public readonly dispatch = output<void>();
  public readonly cancelTrip = output<string | undefined>();

  // Computed values
  protected canDispatch(): boolean {
    return this.status() === "draft" && !!this.truckId();
  }

  protected canCancel(): boolean {
    const status = this.status();
    return status === "draft" || status === "dispatched" || status === "in_transit";
  }

  protected showEdit(): boolean {
    const status = this.status();
    return status === "draft";
  }

  protected editLink(): string[] {
    return ["/trips", this.tripId() ?? "", "edit"];
  }

  protected onDispatch(): void {
    this.toastService.confirm({
      message: "Are you sure you want to dispatch this trip?",
      header: "Confirm Dispatch",
      icon: "send",
      acceptLabel: "Dispatch",
      rejectLabel: "Cancel",
      severity: "success",
      accept: () => {
        this.dispatch.emit();
      },
    });
  }

  protected onCancelClick(): void {
    this.toastService.confirm({
      message: "Are you sure you want to cancel this trip? This action cannot be undone.",
      header: "Confirm Cancellation",
      icon: "warning",
      acceptLabel: "Cancel Trip",
      rejectLabel: "Keep Trip",
      severity: "danger",
      accept: () => {
        this.cancelTrip.emit(undefined);
      },
    });
  }
}
