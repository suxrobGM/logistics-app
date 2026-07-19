import { DecimalPipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { ActivatedRoute, RouterLink } from "@angular/router";
import { type FuelCardTransactionDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Badge,
  DateRangePicker,
  EmptyState,
  ErrorState,
  SearchField,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiSelectField,
  UiTableRowDirectives,
  UiToggleGroup,
  UiTooltip,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import {
  AssignTruckDialog,
  FUEL_CARD_PROVIDER_OPTIONS,
  getFuelCardProviderLabel,
  getTransactionStatusSeverity,
  type AssignTruckRequest,
} from "../components";
import { FuelCardQueueStore } from "../store";

@Component({
  selector: "app-fuel-card-transactions",
  templateUrl: "./fuel-card-transactions.html",
  providers: [FuelCardQueueStore],
  imports: [
    AssignTruckDialog,
    Badge,
    CurrencyFormatPipe,
    DateFormatPipe,
    DateRangePicker,
    DecimalPipe,
    EmptyState,
    ErrorState,
    PageHeader,
    RouterLink,
    SearchField,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    UiSelectField,
    UiTableRowDirectives,
    UiToggleGroup,
    UiTooltip,
  ],
})
export class FuelCardTransactionsComponent implements OnInit {
  protected readonly store = inject(FuelCardQueueStore);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly saving = signal(false);
  protected readonly showAssignDialog = signal(false);
  private readonly assignTargets = signal<FuelCardTransactionDto[]>([]);
  protected readonly assignSummary = computed(() =>
    this.store.buildAssignSummary(this.assignTargets()),
  );
  protected readonly assignSuggestedTruckId = computed(() =>
    this.store.suggestTruckId(this.assignTargets()),
  );

  protected readonly providerOptions = [
    { label: "All providers", value: null },
    ...FUEL_CARD_PROVIDER_OPTIONS.map((o) => ({ label: o.label, value: o.value })),
  ];
  protected readonly getProviderLabel = getFuelCardProviderLabel;
  protected readonly getStatusSeverity = getTransactionStatusSeverity;

  ngOnInit(): void {
    void this.store.init(this.route.snapshot.queryParamMap.get("status"));
  }

  protected openAssignDialog(transaction: FuelCardTransactionDto): void {
    this.openAssign([transaction]);
  }

  protected openBulkAssign(): void {
    this.openAssign(this.store.selection());
  }

  private openAssign(targets: FuelCardTransactionDto[]): void {
    if (!targets.length) {
      return;
    }
    this.assignTargets.set(targets);
    this.showAssignDialog.set(true);
  }

  protected async onAssign(request: AssignTruckRequest): Promise<void> {
    this.saving.set(true);
    try {
      await this.store.assign(this.assignTargets(), request);
      this.showAssignDialog.set(false);
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
      accept: () => void this.store.ignore([transaction]),
    });
  }

  protected bulkIgnore(): void {
    const selected = this.store.selection();
    if (!selected.length) {
      return;
    }
    const noun = selected.length === 1 ? "transaction" : "transactions";
    this.toast.confirm({
      header: "Ignore Transactions",
      message: `Ignore ${selected.length} selected ${noun}? They will leave the review queue without creating expenses.`,
      icon: "warning",
      accept: () => void this.store.ignore(selected),
    });
  }
}
