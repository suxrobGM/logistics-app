import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { type PostedTruckDto } from "@logistics/shared/api";
import { Badge, Icon, Stack, UiButton, UiDataTable, UiTooltip } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { getPostedTruckStatusSeverity } from "../loadboard.constants";

@Component({
  selector: "app-posted-trucks-table",
  templateUrl: "./posted-trucks-table.html",
  imports: [Badge, DatePipe, Icon, Stack, UiButton, UiDataTable, UiTooltip],
})
export class PostedTrucksTable {
  private readonly toast = inject(ToastService);

  public readonly trucks = input.required<PostedTruckDto[]>();
  public readonly remove = output<string>();

  protected readonly getStatusSeverity = getPostedTruckStatusSeverity;

  protected confirmRemove(truck: PostedTruckDto): void {
    this.toast.confirm({
      message: `Are you sure you want to remove this truck post from ${truck.providerName}?`,
      header: "Remove Posted Truck",
      icon: "warning",
      severity: "danger",
      accept: () => this.remove.emit(truck.id!),
    });
  }
}
