import {CommonModule} from "@angular/common";
import {Component, model, signal} from "@angular/core";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {SubscriptionPaymentDto} from "@/core/api/models";
import {AddressPipe} from "@/core/pipes";
import {TenantService} from "@/core/services";

@Component({
  selector: "app-billing-history",
  templateUrl: "./billing-history.component.html",
  imports: [CommonModule, TableModule, TagModule, AddressPipe],
})
export class BillingHistoryComponent {
  readonly payments = signal<SubscriptionPaymentDto[]>([]);
  readonly isLoading = signal(false);
  readonly totalRecords = signal(0);
  readonly first = model(0);

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService
  ) {}

  load(event: TableLazyLoadEvent) {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField as string, event.sortOrder);
    const subscriptionId = this.tenantService.getTenantData()?.subscription?.id;

    this.apiService
      .getSubscriptionPayments({
        subscriptionId: subscriptionId,
        orderBy: sortField,
        page: page,
        pageSize: rows,
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.payments.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoading.set(false);
      });
  }
}
