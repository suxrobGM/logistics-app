import {CurrencyPipe} from "@angular/common";
import {Component, computed, effect, input, model, output, signal, viewChild} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {Table, TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {CreateTripLoadCommand, LoadStatus, TripLoadDto} from "@/core/api/models";
import {LoadFormValue, LoadStatusTag} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";
import {TripLoadDialog} from "../trip-load-dialog/trip-load-dialog";

interface TableRow extends TripLoadDto {
  kind: "existing" | "new";
  pendingDetach?: boolean;
}

export interface LoadsStepData {
  newLoads: CreateTripLoadCommand[];
  attachedLoads: TripLoadDto[];
  detachedLoads: TripLoadDto[];
}

interface NewLoad extends CreateTripLoadCommand {
  id: string;
}

@Component({
  selector: "app-trip-form-step-loads",
  templateUrl: "./trip-form-step-loads.html",
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
    TripLoadDialog,
  ],
})
export class TripFormStepLoads {
  private readonly newLoads: NewLoad[] = [];
  private readonly attachedLoads: TripLoadDto[] = [];
  private readonly detachedLoads: TripLoadDto[] = [];

  public readonly truckId = input<string>();
  public readonly initialData = input<TripLoadDto[] | null>(null);

  public readonly back = output<void>();
  public readonly next = output<LoadsStepData>();

  protected readonly dataTable = viewChild<Table<TableRow>>("dataTable");
  protected readonly tripLoadDialogVisible = model(false);
  protected readonly rows = signal<TableRow[]>([]);

  protected readonly totalDistance = computed(() =>
    this.rows().reduce((total, load) => total + load.distance, 0)
  );
  protected readonly totalCost = computed(() =>
    this.rows().reduce((total, load) => total + load.deliveryCost, 0)
  );

  constructor() {
    effect(() => {
      const initialData = this.initialData();

      if (initialData) {
        this.rows.set(
          initialData.map((load) => ({...load, kind: "existing", pendingDetach: false}))
        );
      }
    });
  }

  protected getAssignedLoadsCount(): string {
    return (this.rows()?.length ?? 0).toString();
  }

  protected attachLoad(load: TripLoadDto): void {
    this.attachedLoads.push(load);
  }

  protected detachLoad(load: TripLoadDto): void {
    this.detachedLoads.push(load);

    this.rows.update((rows) =>
      rows.map((row) => (row.id === load.id ? {...row, pendingDetach: true} : row))
    );
  }

  protected undoDetachLoad(load: TripLoadDto): void {
    this.detachedLoads.splice(this.detachedLoads.indexOf(load), 1);
    this.rows.update((rows) =>
      rows.map((row) => (row.id === load.id ? {...row, pendingDetach: false} : row))
    );
  }

  protected addNewLoad(load: LoadFormValue): void {
    const tempId = crypto.randomUUID();
    this.newLoads.push({
      ...load,
      id: tempId,
    });

    this.rows.update((rows) => [
      ...rows,
      {
        ...load,
        kind: "new",
        id: tempId,
        number: 0,
        status: LoadStatus.Dispatched,
        pendingDetach: false,
      } as TableRow,
    ]);
  }

  protected removeNewLoad(load: TripLoadDto): void {
    const existingLoadIndex = this.newLoads.findIndex((l) => l.id === load.id);
    this.newLoads.splice(existingLoadIndex, 1);
    this.rows.update((rows) => rows.filter((row) => row.id !== load.id));
  }

  protected goToNextStep(): void {
    this.next.emit({
      newLoads: this.newLoads,
      attachedLoads: this.attachedLoads,
      detachedLoads: this.detachedLoads,
    });
  }

  protected applyFilter(event: Event): void {
    this.dataTable()?.filterGlobal((event.target as HTMLInputElement).value, "contains");
  }
}
