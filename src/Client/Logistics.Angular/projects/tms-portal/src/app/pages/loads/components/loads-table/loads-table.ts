import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import type { AppError } from "@logistics/shared";
import type { LoadDto, LoadStatus } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import type { ListLazyLoadEvent } from "@logistics/shared/stores";
import {
  Card,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  UiTableRowDirectives,
  UiTooltip,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { DataContainer, LoadStatusTag, LoadTypeTag, RouteBadge } from "@/shared/components";

@Component({
  selector: "app-loads-table",
  templateUrl: "./loads-table.html",
  imports: [
    Card,
    CurrencyFormatPipe,
    DataContainer,
    DateFormatPipe,
    DistanceUnitPipe,
    LoadStatusTag,
    LoadTypeTag,
    RouteBadge,
    RouterLink,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
    UiTableRowDirectives,
    UiTooltip,
  ],
})
export class LoadsTable {
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly localizationService = inject(LocalizationService);

  // Localization
  protected readonly distanceUnitLabel = computed(() =>
    this.localizationService.getDistanceUnitLabel(),
  );

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
  public readonly lazyLoad = output<ListLazyLoadEvent>();
  public readonly retry = output<void>();
  public readonly addLoad = output<void>();
  public readonly assignLoad = output<LoadDto>();
  public readonly dispatchLoad = output<LoadDto>();

  // Internal state
  protected readonly selectedRow = signal<LoadDto | null>(null);

  protected readonly actionMenuItems = computed<UiMenuItem[]>(() => {
    const row = this.selectedRow();
    const isEditable = row?.status !== "delivered" && row?.status !== "cancelled";

    return [
      {
        label: "View details",
        icon: "eye",
        command: () => this.router.navigateByUrl(`/loads/${row!.id}`),
      },
      {
        label: "Edit load details",
        icon: "square-pen",
        command: () => this.router.navigateByUrl(`/loads/${row!.id}/edit`),
        visible: isEditable,
      },
      { separator: true },
      {
        label: "Assign to Truck",
        icon: "truck",
        command: () => this.assignLoad.emit(row!),
        visible: isEditable,
      },
      {
        label: "Dispatch",
        icon: "send",
        command: () => this.onDispatchLoad(row!),
        visible: row?.status === "draft",
      },
      { separator: true },
      {
        label: "View truck details",
        icon: "navigation",
        command: () => this.router.navigateByUrl(`/trucks/${row!.assignedTruckId}`),
        visible: !!row?.assignedTruckId,
      },
      {
        label: "View invoice",
        icon: "book",
        command: () => this.router.navigateByUrl(`/invoices/loads/${row!.id}/${row!.invoice?.id}`),
        visible: !!row?.invoice?.id,
      },
    ];
  });

  protected onLazyLoad(event: ListLazyLoadEvent): void {
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

  protected navigateToLoad(load: LoadDto): void {
    this.router.navigateByUrl(`/loads/${load.id}`);
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
