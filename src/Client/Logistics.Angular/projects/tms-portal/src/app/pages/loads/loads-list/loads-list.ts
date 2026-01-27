import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import {
  type Address,
  Api,
  type CustomerDto,
  type LoadDto,
  type LoadStatus,
  type LoadType,
  type TruckDto,
  bulkDeleteLoads,
} from "@logistics/shared/api";
import { loadStatusOptions, loadTypeOptions } from "@logistics/shared/api/enums";
import { AddressPipe } from "@logistics/shared/pipes";
import { downloadBlobFile } from "@logistics/shared/utils";
import type { MenuItem, SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { MenuModule } from "primeng/menu";
import { MultiSelectModule } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import {
  DataContainer,
  DateRangePicker,
  LabeledField,
  LoadStatusTag,
  LoadTypeTag,
  SearchCustomer,
  SearchInput,
  SearchTruck,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { BulkAssignDialog, LoadsSummaryStats } from "../components";
import { LoadsListStore } from "../store/loads-list.store";

@Component({
  selector: "app-loads-list",
  templateUrl: "./loads-list.html",
  providers: [LoadsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    DistanceUnitPipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
    MenuModule,
    DataContainer,
    DatePipe,
    CurrencyPipe,
    SearchInput,
    FormsModule,
    CheckboxModule,
    DateRangePicker,
    MultiSelectModule,
    SearchTruck,
    SearchCustomer,
    LabeledField,
    BulkAssignDialog,
    LoadsSummaryStats,
  ],
})
export class LoadsListComponent {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(LoadsListStore);

  // Filter state
  protected readonly selectedStatuses = signal<LoadStatus[]>([]);
  protected readonly selectedTypes = signal<LoadType[]>([]);
  protected readonly selectedTruck = signal<TruckDto | null>(null);
  protected readonly selectedCustomer = signal<CustomerDto | null>(null);
  protected readonly onlyActiveLoads = signal<boolean>(false);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Bulk selection state
  protected readonly selectedLoads = signal<LoadDto[]>([]);
  protected readonly showBulkAssignDialog = signal(false);
  protected readonly isBulkProcessing = signal(false);

  // Filter options
  protected readonly statusOptions: SelectItem[] = loadStatusOptions;
  protected readonly typeOptions: SelectItem[] = loadTypeOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedTypes().length > 0) count++;
    if (this.selectedTruck()) count++;
    if (this.selectedCustomer()) count++;
    if (this.onlyActiveLoads()) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  // Computed: check if any selected loads can be dispatched (Draft status only)
  protected readonly canDispatchSelected = computed(() =>
    this.selectedLoads().some((load) => load.status === "draft"),
  );

  // Computed: check if any selected loads can be deleted (Draft status only)
  protected readonly canDeleteSelected = computed(() =>
    this.selectedLoads().some((load) => load.status === "draft"),
  );

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
        command: () => this.quickAssignLoad(),
        visible: isEditable,
      },
      {
        label: "Dispatch",
        icon: "pi pi-send",
        command: () => this.quickDispatchLoad(),
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
        label: "View invoices",
        icon: "pi pi-book",
        command: () => this.router.navigateByUrl(`/invoices/loads/${row!.id}`),
      },
    ];
  });

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addLoad(): void {
    this.router.navigate(["/loads/add"]);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    // Status filter (supports multiple)
    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Statuses"] = statuses;
    }

    // Type filter (supports multiple)
    const types = this.selectedTypes();
    if (types.length > 0) {
      filters["Types"] = types;
    }

    // Truck filter
    const truck = this.selectedTruck();
    if (truck?.id) {
      filters["TruckId"] = truck.id;
    }

    // Customer filter
    const customer = this.selectedCustomer();
    if (customer?.id) {
      filters["CustomerId"] = customer.id;
    }

    // Active only filter
    filters["OnlyActiveLoads"] = this.onlyActiveLoads();

    // Date range filter
    const range = this.dateRange();
    if (range?.length === 2) {
      filters["StartDate"] = range[0].toISOString();
      filters["EndDate"] = range[1].toISOString();
    }

    this.store.setFilters(filters);
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedTypes.set([]);
    this.selectedTruck.set(null);
    this.selectedCustomer.set(null);
    this.onlyActiveLoads.set(false);
    this.dateRange.set(null);
    this.store.setFilters({});
  }

  protected onDateRangeChange(dates: Date[]): void {
    this.dateRange.set(dates);
    this.applyFilters();
  }

  protected exportToCsv(): void {
    const loads = this.store.data();
    if (loads.length === 0) {
      this.toastService.showWarning("No loads to export");
      return;
    }

    const headers = [
      "Number",
      "Name",
      "Type",
      "Customer",
      "Origin",
      "Destination",
      "Status",
      "Distance (mi)",
      "Cost",
      "Truck",
      "Dispatcher",
    ];

    const rows = loads.map((load) => [
      load.number ?? "",
      load.name ?? "",
      load.type ?? "",
      load.customer?.name ?? "",
      this.formatAddress(load.originAddress),
      this.formatAddress(load.destinationAddress),
      load.status ?? "",
      load.distance ?? "",
      load.deliveryCost ?? "",
      load.assignedTruckNumber ?? "",
      load.assignedDispatcherName ?? "",
    ]);

    const csvContent = [headers, ...rows]
      .map((row) => row.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    downloadBlobFile(blob, `loads-export-${new Date().toISOString().split("T")[0]}.csv`);
    this.toastService.showSuccess(`Exported ${loads.length} load(s) to CSV`);
  }

  protected clearSelection(): void {
    this.selectedLoads.set([]);
  }

  protected openBulkAssignDialog(): void {
    this.showBulkAssignDialog.set(true);
  }

  protected onBulkAssigned(): void {
    this.clearSelection();
    this.store.retry();
  }

  protected quickAssignLoad(): void {
    const load = this.selectedRow();
    if (load) {
      this.selectedLoads.set([load]);
      this.showBulkAssignDialog.set(true);
    }
  }

  protected quickDispatchLoad(): void {
    const load = this.selectedRow();
    if (!load || load.status !== "draft") {
      this.toastService.showWarning("Only Draft loads can be dispatched");
      return;
    }

    this.toastService.confirm({
      header: "Dispatch Load",
      message: `Are you sure you want to dispatch load #${load.number}?`,
      accept: async () => {
        // TODO: Call dispatch API after regeneration
        // await this.api.invoke(dispatchLoad, { id: load.id });
        this.toastService.showSuccess(`Load #${load.number} has been dispatched`);
        this.store.retry();
      },
    });
  }

  protected async bulkDispatch(): Promise<void> {
    const draftLoads = this.selectedLoads().filter((l) => l.status === "draft");
    if (draftLoads.length === 0) {
      this.toastService.showWarning("No loads in Draft status to dispatch");
      return;
    }

    this.toastService.confirm({
      header: "Dispatch Loads",
      message: `Are you sure you want to dispatch ${draftLoads.length} load(s)?`,
      accept: async () => {
        this.isBulkProcessing.set(true);
        try {
          // TODO: Call bulk dispatch API after regeneration
          // await this.api.invoke(bulkDispatchLoads, { body: { loadIds: draftLoads.map(l => l.id) } });
          this.toastService.showSuccess(`Dispatched ${draftLoads.length} load(s)`);
          this.clearSelection();
          this.store.retry();
        } finally {
          this.isBulkProcessing.set(false);
        }
      },
    });
  }

  protected async bulkDelete(): Promise<void> {
    const draftLoads = this.selectedLoads().filter((l) => l.status === "draft");
    if (draftLoads.length === 0) {
      this.toastService.showWarning("Only Draft loads can be deleted");
      return;
    }

    this.toastService.confirm({
      header: "Delete Loads",
      message: `Are you sure you want to delete ${draftLoads.length} load(s)? This action cannot be undone.`,
      accept: async () => {
        this.isBulkProcessing.set(true);
        try {
          // TODO: Call bulk delete API after regeneration
          await this.api.invoke(bulkDeleteLoads, {
            body: { loadIds: draftLoads.map((l) => l.id).filter((id): id is string => !!id) },
          });
          this.toastService.showSuccess(`Deleted ${draftLoads.length} load(s)`);
          this.clearSelection();
          this.store.retry();
        } finally {
          this.isBulkProcessing.set(false);
        }
      },
    });
  }

  private formatAddress(address: Address | null | undefined): string {
    if (!address) return "";
    const parts = [address.line1, address.city, address.state].filter(Boolean);
    return parts.join(", ");
  }

  protected getStatusBorderClass(status: LoadStatus | undefined): string {
    switch (status) {
      case "draft":
        return "border-l-4 border-l-gray-400";
      case "dispatched":
        return "border-l-4 border-l-blue-500";
      case "picked_up":
        return "border-l-4 border-l-orange-500";
      case "delivered":
        return "border-l-4 border-l-green-500";
      case "cancelled":
        return "border-l-4 border-l-red-500";
      default:
        return "";
    }
  }
}
