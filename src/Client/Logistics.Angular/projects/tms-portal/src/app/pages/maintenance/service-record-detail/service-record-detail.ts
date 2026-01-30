import { Component, computed, inject, input, type OnInit, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { CurrencyPipe, DatePipe, DecimalPipe } from "@angular/common";
import { Api, getMaintenanceRecords } from "@logistics/shared/api";
import type { MaintenanceRecordDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TagModule } from "primeng/tag";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-service-record-detail",
  templateUrl: "./service-record-detail.html",
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    TagModule,
    PageHeader,
  ],
})
export class ServiceRecordDetailPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly record = signal<MaintenanceRecordDto | null>(null);

  protected readonly pageTitle = computed(() => {
    const rec = this.record();
    if (!rec) return "Service Record";
    return `${rec.typeDisplay ?? "Service"} - ${rec.truckNumber ?? "Unknown Truck"}`;
  });

  async ngOnInit(): Promise<void> {
    await this.loadRecord();
  }

  private async loadRecord(): Promise<void> {
    this.isLoading.set(true);
    try {
      // TODO: Use getMaintenanceRecordById once API is regenerated
      // For now, fetch from list and filter
      const result = await this.api.invoke(getMaintenanceRecords, {});
      const found = result?.items?.find((r: MaintenanceRecordDto) => r.id === this.id());

      if (found) {
        this.record.set(found);
      } else {
        this.toastService.showError("Maintenance record not found");
        this.router.navigateByUrl("/maintenance/records");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected editRecord(): void {
    this.router.navigateByUrl(`/maintenance/records/${this.id()}/edit`);
  }
}
