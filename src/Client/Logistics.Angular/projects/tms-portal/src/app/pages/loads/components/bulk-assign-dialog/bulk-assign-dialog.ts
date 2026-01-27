import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject, input, model, output, signal } from "@angular/core";
import { Api, bulkAssignLoads } from "@logistics/shared/api";
import type { LoadDto, TruckDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { SearchTruck } from "@/shared/components";

@Component({
  selector: "app-bulk-assign-dialog",
  templateUrl: "./bulk-assign-dialog.html",
  imports: [DialogModule, ProgressSpinnerModule, ButtonModule, SearchTruck, CurrencyPipe],
})
export class BulkAssignDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly loads = input.required<LoadDto[]>();
  public readonly visible = model<boolean>(false);
  public readonly assigned = output<void>();

  protected readonly selectedTruck = signal<TruckDto | null>(null);
  protected readonly isAssigning = signal(false);

  protected readonly totalCost = computed(() =>
    this.loads().reduce((sum, load) => sum + (load.deliveryCost ?? 0), 0),
  );

  async assign(): Promise<void> {
    const truck = this.selectedTruck();
    if (!truck) {
      this.toastService.showWarning("Please select a truck");
      return;
    }

    this.isAssigning.set(true);
    try {
      await this.api.invoke(bulkAssignLoads, {
        body: {
          loadIds: this.loads().map((l) => l.id!),
          truckId: truck.id!,
        },
      });
      this.toastService.showSuccess(
        `Assigned ${this.loads().length} load(s) to truck #${truck.number}`,
      );
      this.assigned.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to assign loads");
    } finally {
      this.isAssigning.set(false);
    }
  }

  close(): void {
    this.selectedTruck.set(null);
    this.visible.set(false);
  }
}
