import { Component, computed, inject, input, output } from "@angular/core";
import { Api, type LoadDto, bulkDeleteLoads, bulkDispatchLoads } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-loads-bulk-toolbar",
  templateUrl: "./loads-bulk-toolbar.html",
  imports: [ButtonModule, TooltipModule],
})
export class LoadsBulkToolbar {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly selectedLoads = input.required<LoadDto[]>();
  public readonly isProcessing = input(false);

  public readonly assignRequested = output<void>();
  public readonly dispatched = output<void>();
  public readonly deleted = output<void>();
  public readonly selectionCleared = output<void>();

  // Computed: check if any selected loads can be dispatched (Draft status only)
  protected readonly canDispatchSelected = computed(() =>
    this.selectedLoads().some((load) => load.status === "draft"),
  );

  // Computed: check if any selected loads can be deleted (Draft status only)
  protected readonly canDeleteSelected = computed(() =>
    this.selectedLoads().some((load) => load.status === "draft"),
  );

  protected openBulkAssignDialog(): void {
    this.assignRequested.emit();
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
        try {
          await this.api.invoke(bulkDispatchLoads, {
            body: { loadIds: draftLoads.map((l) => l.id).filter((id): id is string => !!id) },
          });
          this.toastService.showSuccess(`Dispatched ${draftLoads.length} load(s)`);
          this.dispatched.emit();
        } catch {
          this.toastService.showError("Failed to dispatch loads");
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
        await this.api.invoke(bulkDeleteLoads, {
          body: { loadIds: draftLoads.map((l) => l.id).filter((id): id is string => !!id) },
        });
        this.toastService.showSuccess(`Deleted ${draftLoads.length} load(s)`);
        this.deleted.emit();
      },
    });
  }

  protected clearSelection(): void {
    this.selectionCleared.emit();
  }
}
