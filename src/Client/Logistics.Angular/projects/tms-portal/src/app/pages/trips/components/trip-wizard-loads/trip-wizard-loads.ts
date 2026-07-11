import { Component, computed, inject, input, model, viewChild } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Badge,
  Icon,
  SearchField,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiDialog,
  UiSortHeader,
  UiTooltip,
} from "@logistics/shared/ui";
import { LoadForm, LoadStatusTag, type LoadFormValue } from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { TripWizardStore, type TableRow } from "../../store/trip-wizard-store";
import { AttachLoadDialog } from "../attach-load-dialog/attach-load-dialog";

@Component({
  selector: "app-trip-wizard-loads",
  templateUrl: "./trip-wizard-loads.html",
  imports: [
    AddressPipe,
    AttachLoadDialog,
    Badge,
    CurrencyFormatPipe,
    DistanceUnitPipe,
    Icon,
    LoadForm,
    LoadStatusTag,
    RouterLink,
    SearchField,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiDialog,
    UiSortHeader,
    UiTooltip,
  ],
})
export class TripFormStepLoads {
  protected readonly store = inject(TripWizardStore);

  protected readonly dataTable = viewChild<UiDataTable<TableRow>>("dataTable");
  protected readonly tripLoadDialogVisible = model(false);
  protected readonly attachLoadDialogVisible = model(false);

  // Expose store computed values for template
  protected readonly rows = this.store.tableRows;
  protected readonly totalLoads = this.store.totalLoads;

  // Load IDs already in the trip (to exclude from attach dialog)
  protected readonly excludeLoadIds = computed(() =>
    this.rows()
      .map((row) => row.id)
      .filter((id): id is string => !!id),
  );

  protected readonly initialLoadData = computed(
    () =>
      ({
        assignedTruckId: this.store.truckId(),
        type: "vehicle",
      }) satisfies Partial<LoadFormValue>,
  );

  public readonly disabled = input<boolean>(false);

  protected getAssignedLoadsCount(): string {
    return this.totalLoads().toString();
  }

  protected detachLoad(loadId: string): void {
    this.store.detachLoad(loadId);
  }

  protected undoDetachLoad(loadId: string): void {
    this.store.undoDetachLoad(loadId);
  }

  protected addNewLoad(load: LoadFormValue): void {
    this.store.addNewLoad({
      ...load,
      customerId: load.customer?.id ?? "",
    });
    this.tripLoadDialogVisible.set(false);
  }

  protected removeNewLoad(loadId: string): void {
    this.store.removeNewLoad(loadId);
  }

  protected attachExistingLoad(load: LoadDto): void {
    // Convert LoadDto to TripLoadDto format for the store
    this.store.attachExistingLoad({
      id: load.id!,
      name: load.name!,
      number: load.number ?? 0,
      type: load.type,
      status: load.status!,
      originAddress: load.originAddress!,
      originLocation: load.originLocation!,
      destinationAddress: load.destinationAddress!,
      destinationLocation: load.destinationLocation!,
      deliveryCost: load.deliveryCost,
      distance: load.distance,
      customer: load.customer ?? { id: "", name: "" },
    });
  }

  protected goToPreviousStep(): void {
    this.store.previousStep();
  }

  protected goToNextStep(): void {
    // Check if stops need regeneration (loads have changed)
    const needsRegeneration = this.store.stopsNeedRegeneration();

    if (needsRegeneration) {
      // Optimize stops first, then navigate
      this.store.optimizeStopsFromStep2();
    }

    // Navigate to next step
    this.store.nextStep();
  }

  protected applyFilter(value: string): void {
    this.dataTable()?.filterGlobal(value, "contains");
  }
}
