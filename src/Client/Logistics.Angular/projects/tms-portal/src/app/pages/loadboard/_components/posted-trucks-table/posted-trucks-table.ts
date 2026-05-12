import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { type PostedTruckDto } from "@logistics/shared/api";
import { Icon, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { getPostedTruckStatusSeverity } from "../loadboard.constants";

@Component({
  selector: "app-posted-trucks-table",
  templateUrl: "./posted-trucks-table.html",
  imports: [ButtonModule, DatePipe, Icon, Stack, TableModule, TagModule, TooltipModule],
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
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.remove.emit(truck.id!),
    });
  }
}
