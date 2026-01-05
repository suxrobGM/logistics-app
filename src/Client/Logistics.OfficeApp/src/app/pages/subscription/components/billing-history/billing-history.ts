import { CommonModule } from "@angular/common";
import { Component, inject, model, signal } from "@angular/core";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { Api, formatSortField, getPayments$Json } from "@/core/api";
import { PaymentDto } from "@/core/api/models";
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

    const result = await this.api.invoke(getPayments$Json, {
      SubscriptionId: subscriptionId,
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
      StartDate: new Date().toISOString(),
    });
    if (result.success && result.data) {
      this.payments.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
    }

    this.isLoading.set(false);
  }
}
