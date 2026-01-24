import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject, input, model, viewChild } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { Dialog } from "primeng/dialog";
import { IconField } from "primeng/iconfield";
import { InputIcon } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { Table, TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { LoadFormComponent, type LoadFormValue, LoadStatusTag } from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { type TableRow, TripWizardStore } from "../../store/trip-wizard-store";

@Component({
  selector: "app-trip-wizard-loads",
  templateUrl: "./trip-wizard-loads.html",
  imports: [
    FormsModule,
    ButtonModule,
    TableModule,
    TagModule,
    TooltipModule,
    AddressPipe,
    DistanceUnitPipe,
    CurrencyPipe,
    LoadStatusTag,
    RouterLink,
    InputTextModule,
    IconField,
    InputIcon,
    Dialog,
    LoadFormComponent,
  ],
})
export class TripFormStepLoads {
  protected readonly store = inject(TripWizardStore);

  protected readonly dataTable = viewChild<Table<TableRow>>("dataTable");
  protected readonly tripLoadDialogVisible = model(false);

  // Expose store computed values for template
  protected readonly rows = this.store.tableRows;
  protected readonly totalLoads = this.store.totalLoads;

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

  protected applyFilter(event: Event): void {
    this.dataTable()?.filterGlobal((event.target as HTMLInputElement).value, "contains");
  }
}
