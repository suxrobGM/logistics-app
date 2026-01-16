import { CommonModule } from "@angular/common";
import { Component, inject, model, signal } from "@angular/core";
import { Api, formatSortField, getPayments } from "@logistics/shared/api";
import type { PaymentDto } from "@logistics/shared/api/models";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TenantService } from "@/core/services";
import { AddressPipe } from "@/shared/pipes";

@Component({
  selector: "app-billing-history",
  templateUrl: "./billing-history.html",
  imports: [CommonModule, TableModule, TagModule, AddressPipe],
})
export class BillingHistoryComponent {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  protected readonly payments = signal<PaymentDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = model(0);

  protected async load(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);
    const subscriptionId = this.tenantService.getTenantData()?.subscription?.id;

    const result = await this.api.invoke(getPayments, {
      SubscriptionId: subscriptionId,
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      StartDate: new Date().toISOString(),
    });
    if (result) {
      this.payments.set(result.items ?? []);
      this.totalRecords.set(result.pagination?.total ?? 0);
    }

    this.isLoading.set(false);
  }
}
