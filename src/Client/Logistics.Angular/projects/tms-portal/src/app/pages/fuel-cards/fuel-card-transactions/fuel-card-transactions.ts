import { DecimalPipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  assignFuelCardTransactionTruck,
  getFuelCardTransactions,
  getTrucks,
  ignoreFuelCardTransaction,
  type FuelCardTransactionDto,
  type FuelCardTransactionStatus,
  type TruckDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Badge,
  EmptyState,
  ErrorState,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiToggleGroup,
  UiTooltip,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import {
  AssignTruckDialog,
  getFuelCardProviderLabel,
  getTransactionStatusSeverity,
  type AssignTruckRequest,
} from "../_components";

type StatusFilter = FuelCardTransactionStatus | "all";

@Component({
  selector: "app-fuel-card-transactions",
  templateUrl: "./fuel-card-transactions.html",
  imports: [
    AssignTruckDialog,
    Badge,
    CurrencyFormatPipe,
    DateFormatPipe,
    DecimalPipe,
    EmptyState,
    ErrorState,
    PageHeader,
    RouterLink,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    UiToggleGroup,
    UiTooltip,
  ],
})
export class FuelCardTransactionsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly transactions = signal<FuelCardTransactionDto[]>([]);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly statusFilter = signal<StatusFilter>("pending");
  protected readonly showAssignDialog = signal(false);
  protected readonly selectedTransaction = signal<FuelCardTransactionDto | null>(null);

  protected readonly statusOptions = [
    { label: "Pending", value: "pending" },
    { label: "Matched", value: "matched" },
    { label: "Ignored", value: "ignored" },
    { label: "All", value: "all" },
  ];

  protected readonly getProviderLabel = getFuelCardProviderLabel;
  protected readonly getStatusSeverity = getTransactionStatusSeverity;

  ngOnInit(): void {
    void this.loadTransactions();
    void this.loadTrucks();
  }

  protected async loadTransactions(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const status = this.statusFilter();
      const data = await this.api.invoke(getFuelCardTransactions, {
        Status: status === "all" ? undefined : status,
        PageSize: 200,
      });
      this.transactions.set(data?.value ?? []);
    } catch (err) {
      this.error.set("Failed to load fuel card transactions");
      console.error("Error loading transactions:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected onStatusFilterChange(status: StatusFilter): void {
    this.statusFilter.set(status);
    void this.loadTransactions();
  }

  protected openAssignDialog(transaction: FuelCardTransactionDto): void {
    this.selectedTransaction.set(transaction);
    this.showAssignDialog.set(true);
  }

  protected async onAssign(request: AssignTruckRequest): Promise<void> {
    this.saving.set(true);
    try {
      await this.api.invoke(assignFuelCardTransactionTruck, {
        transactionId: request.transactionId,
        body: {
          transactionId: request.transactionId,
          truckId: request.truckId,
          rememberMapping: request.rememberMapping,
        },
      });
      this.showAssignDialog.set(false);
      this.toast.showSuccess("Transaction assigned. A paid fuel expense has been created.");
      await this.loadTransactions();
    } catch (err) {
      console.error("Error assigning transaction:", err);
      this.toast.showError("Failed to assign transaction");
    } finally {
      this.saving.set(false);
    }
  }

  protected ignoreTransaction(transaction: FuelCardTransactionDto): void {
    this.toast.confirm({
      message:
        "Ignore this transaction? It will leave the review queue without creating an expense.",
      header: "Ignore Transaction",
      icon: "warning",
      accept: () => void this.doIgnore(transaction),
    });
  }

  private async doIgnore(transaction: FuelCardTransactionDto): Promise<void> {
    try {
      await this.api.invoke(ignoreFuelCardTransaction, { transactionId: transaction.id! });
      await this.loadTransactions();
    } catch (err) {
      console.error("Error ignoring transaction:", err);
      this.toast.showError("Failed to ignore transaction");
    }
  }

  private async loadTrucks(): Promise<void> {
    try {
      const data = await this.api.invoke(getTrucks, {});
      this.trucks.set(data?.items ?? []);
    } catch (err) {
      console.error("Error loading trucks:", err);
    }
  }
}
