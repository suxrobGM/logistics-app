import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Api, type LoadDto, getUnassignedLoads } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { Dialog } from "primeng/dialog";
import { IconField } from "primeng/iconfield";
import { InputIcon } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { LoadTypeTag } from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";

@Component({
  selector: "app-attach-load-dialog",
  templateUrl: "./attach-load-dialog.html",
  imports: [
    FormsModule,
    ButtonModule,
    Dialog,
    IconField,
    InputIcon,
    InputTextModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule,
    AddressPipe,
    CurrencyPipe,
    LoadTypeTag,
    DistanceUnitPipe,
  ],
})
export class AttachLoadDialog {
  private readonly api = inject(Api);

  public readonly visible = model<boolean>(false);
  public readonly excludeLoadIds = model<string[]>([]);
  public readonly loadAttached = output<LoadDto>();

  protected readonly isLoading = signal(false);
  protected readonly unassignedLoads = signal<LoadDto[]>([]);
  protected readonly searchQuery = signal("");
  protected readonly selectedLoads = signal<LoadDto[]>([]);

  protected readonly filteredLoads = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const excludeIds = new Set(this.excludeLoadIds());

    let loads = this.unassignedLoads().filter((load) => !excludeIds.has(load.id!));

    if (query) {
      loads = loads.filter(
        (load) =>
          load.name?.toLowerCase().includes(query) ||
          load.customer?.name?.toLowerCase().includes(query) ||
          load.originAddress?.city?.toLowerCase().includes(query) ||
          load.destinationAddress?.city?.toLowerCase().includes(query),
      );
    }

    return loads;
  });

  protected readonly totalSelectedDistance = computed(() =>
    this.selectedLoads().reduce((sum, load) => sum + (load.distance ?? 0), 0),
  );

  protected readonly totalSelectedCost = computed(() =>
    this.selectedLoads().reduce((sum, load) => sum + (load.deliveryCost ?? 0), 0),
  );

  protected onDialogShow(): void {
    this.selectedLoads.set([]);
    this.searchQuery.set("");
    this.loadUnassignedLoads();
  }

  protected async loadUnassignedLoads(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getUnassignedLoads, { PageSize: 200 });
      this.unassignedLoads.set(result?.items ?? []);
    } catch (error) {
      console.error("Failed to load unassigned loads:", error);
      this.unassignedLoads.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected attachSelected(): void {
    for (const load of this.selectedLoads()) {
      this.loadAttached.emit(load);
    }
    this.selectedLoads.set([]);
    this.visible.set(false);
  }
}
