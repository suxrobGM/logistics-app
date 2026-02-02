import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { AppError } from "@logistics/shared";
import type { LoadDto, LoadStatus } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-loads-table",
  templateUrl: "./loads-table.html",
  imports: [
    CardModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    MenuModule,
    RouterLink,
    AddressPipe,
    DistanceUnitPipe,
    CurrencyFormatPipe,
    DateFormatPipe,
    LoadStatusTag,
    LoadTypeTag,
    DataContainer,
  ],
})
export class LoadsTable {
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly localizationService = inject(LocalizationService);

  // Localization
  protected readonly distanceUnitLabel = computed(() => this.localizationService.getDistanceUnitLabel());

  // Data inputs
  public readonly data = input.required<LoadDto[]>();
  public readonly isLoading = input(false);
  public readonly error = input<AppError | null>(null);
  public readonly isEmpty = input(false);
  public readonly totalRecords = input(0);
  public readonly pageSize = input(10);
  public readonly first = input(0);

  // Selection
  public readonly selectedLoads = model<LoadDto[]>([]);

  // Outputs
  public readonly lazyLoad = output<TableLazyLoadEvent>();
  public readonly retry = output<void>();
  public readonly addLoad = output<void>();
  public readonly assignLoad = output<LoadDto>();
  public readonly dispatchLoad = output<LoadDto>();

  // Internal state
  protected readonly selectedRow = signal<LoadDto | null>(null);

  protected readonly actionMenuItems = computed<MenuItem[]>(() => {
    const row = this.selectedRow();
    const isEditable = row?.status !== "delivered" && row?.status !== "cancelled";

    return [
      {
        label: "View details",
        icon: "pi pi-eye",
        command: () => this.router.navigateByUrl(`/loads/${row!.id}`),
      },
      {
        label: "Edit load details",
        icon: "pi pi-pen-to-square",
        command: () => this.router.navigateByUrl(`/loads/${row!.id}/edit`),
        visible: isEditable,
      },
      { separator: true },
      {
        label: "Assign to Truck",
        icon: "pi pi-truck",
        command: () => this.assignLoad.emit(row!),
        visible: isEditable,
      },
      {
        label: "Dispatch",
        icon: "pi pi-send",
        command: () => this.onDispatchLoad(row!),
        visible: row?.status === "draft",
      },
      { separator: true },
      {
        label: "View truck details",
        icon: "pi pi-directions",
        command: () => this.router.navigateByUrl(`/trucks/${row!.assignedTruckId}`),
        visible: !!row?.assignedTruckId,
      },
      {
        label: "View invoice",
        icon: "pi pi-book",
        command: () => this.router.navigateByUrl(`/invoices/loads/${row!.id}/${row!.invoice?.id}`),
        visible: !!row?.invoice?.id,
      },
    ];
  });

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    this.lazyLoad.emit(event);
  }

  protected onRetry(): void {
    this.retry.emit();
  }

  protected onAddLoad(): void {
    this.addLoad.emit();
  }

  protected onDispatchLoad(load: LoadDto): void {
    if (load.status !== "draft") {
      this.toastService.showWarning("Only Draft loads can be dispatched");
      return;
    }

    this.toastService.confirm({
      header: "Dispatch Load",
      message: `Are you sure you want to dispatch load #${load.number}?`,
      accept: () => {
        this.dispatchLoad.emit(load);
      },
    });
  }

  protected getStatusBorderClass(status: LoadStatus | undefined): string {
    switch (status) {
      case "draft":
        return "border-l-4 border-l-pending";
      case "dispatched":
        return "border-l-4 border-l-dispatched";
      case "picked_up":
        return "border-l-4 border-l-pickedup";
      case "delivered":
        return "border-l-4 border-l-delivered";
      case "cancelled":
        return "border-l-4 border-l-cancelled";
      default:
        return "";
    }
  }
}
