import {CurrencyPipe} from "@angular/common";
import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  viewChild,
} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {Dialog} from "primeng/dialog";
import {IconField} from "primeng/iconfield";
import {InputIcon} from "primeng/inputicon";
import {InputTextModule} from "primeng/inputtext";
import {Table, TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {
  CreateTripLoadCommand,
  LoadStatus,
  LoadType,
  OptimizeTripStopsCommand,
  TripLoadDto,
  TripStopDto,
  TripStopType,
} from "@/core/api/models";
import {LoadFormComponent, LoadFormValue, LoadStatusTag} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";

interface TableRow extends TripLoadDto {
  kind: "existing" | "new";
  pendingDetach?: boolean;
}

export interface TripWizardLoadsData {
  newLoads: CreateTripLoadCommand[];
  attachedLoads: TripLoadDto[];
  detachedLoads: TripLoadDto[];
  stops: TripStopDto[];
  totalDistance: number;
  totalCost: number;
  totalLoads: number;
  truckId: string;
  truckVehicleCapacity: number;
}

interface NewLoad extends CreateTripLoadCommand {
  id: string;
}

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
  private readonly apiService = inject(ApiService);

  private readonly newLoads: NewLoad[] = [];
  private readonly attachedLoads: TripLoadDto[] = [];
  private readonly detachedLoads: TripLoadDto[] = [];

  protected readonly dataTable = viewChild<Table<TableRow>>("dataTable");
  protected readonly tripLoadDialogVisible = model(false);
  protected readonly rows = signal<TableRow[]>([]);

  protected readonly initialLoadData = computed(
    () =>
      ({
        assignedTruckId: this.stepData()?.truckId,
        type: LoadType.Vehicle,
      }) satisfies Partial<LoadFormValue>
  );

  protected readonly totalDistance = computed(() =>
    this.rows().reduce((total, load) => total + load.distance, 0)
  );
  protected readonly totalCost = computed(() =>
    this.rows().reduce((total, load) => total + load.deliveryCost, 0)
  );

  public readonly stepData = input<TripWizardLoadsData | null>(null);
  public readonly disabled = input<boolean>(false);

  public readonly back = output<void>();
  public readonly next = output<TripWizardLoadsData>();

  constructor() {
    effect(() => {
      const stepData = this.stepData();
      console.log("loads step data", stepData);

      if (stepData) {
        this.initRowsFromStepData(stepData);
      }
    });
  }

  protected getAssignedLoadsCount(): string {
    return (this.rows()?.length ?? 0).toString();
  }

  protected attachLoad(load: TripLoadDto): void {
    this.attachedLoads.push(load);
    this.tripLoadDialogVisible.set(false);
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
      customerId: load.customer?.id ?? "",
    });

    this.rows.update((rows) => [
      ...rows,
      {
        ...load,
        kind: "new",
        id: tempId,
        number: 0,
        status: LoadStatus.Draft,
        pendingDetach: false,
      } as TableRow,
    ]);

    this.tripLoadDialogVisible.set(false);
  }

  protected removeNewLoad(load: TripLoadDto): void {
    const existingLoadIndex = this.newLoads.findIndex((l) => l.id === load.id);
    this.newLoads.splice(existingLoadIndex, 1);
    this.rows.update((rows) => rows.filter((row) => row.id !== load.id));
  }

  protected goToNextStep(): void {
    const {distance, cost, loads} = this.calcTotals();
    const stops = this.buildStops();

    const optimizeTripStopsCommand: OptimizeTripStopsCommand = {
      maxVehicles: this.stepData()!.truckVehicleCapacity!,
      stops: stops,
    };

    // Optimize routes
    this.apiService.tripApi.optimizeTripStops(optimizeTripStopsCommand).subscribe((result) => {
      this.next.emit({
        newLoads: this.newLoads,
        attachedLoads: this.attachedLoads,
        detachedLoads: this.detachedLoads,
        stops: result.data?.orderedStops ?? stops,
        totalDistance: result.data?.totalDistance ?? distance,
        totalCost: cost,
        totalLoads: loads,
        truckId: this.stepData()!.truckId!,
        truckVehicleCapacity: this.stepData()!.truckVehicleCapacity!,
      });
    });
  }

  protected applyFilter(event: Event): void {
    this.dataTable()?.filterGlobal((event.target as HTMLInputElement).value, "contains");
  }

  private initRowsFromStepData(stepData: TripWizardLoadsData): void {
    const existingLoads =
      stepData.attachedLoads?.map(
        (load) =>
          ({
            ...load,
            kind: "existing",
            pendingDetach: false,
          }) satisfies TableRow
      ) ?? [];

    const newLoads =
      stepData.newLoads?.map(
        (load) =>
          ({
            ...load,
            id: crypto.randomUUID(),
            number: 0,
            status: LoadStatus.Draft,
            kind: "new",
            pendingDetach: false,
          }) satisfies TableRow
      ) ?? [];

    this.rows.set([...existingLoads, ...newLoads]);
  }

  private buildStops(): TripStopDto[] {
    const stops: TripStopDto[] = [];
    let order = 1;

    for (const row of this.rows()) {
      if (row.pendingDetach) continue;

      // Pick-up
      stops.push({
        id: crypto.randomUUID(),
        order: order++,
        type: TripStopType.PickUp,
        address: row.originAddress,
        location: {
          longitude: row.originLocation.longitude,
          latitude: row.originLocation.latitude,
        },
        loadId: row.id,
      });

      // Drop-off
      stops.push({
        id: crypto.randomUUID(),
        order: order++,
        type: TripStopType.DropOff,
        address: row.destinationAddress,
        location: {
          longitude: row.destinationLocation.longitude,
          latitude: row.destinationLocation.latitude,
        },
        loadId: row.id,
      });
    }

    return stops;
  }

  private calcTotals(): {distance: number; cost: number; loads: number} {
    let distance = 0;
    let cost = 0;
    let loads = 0;

    for (const row of this.rows()) {
      if (row.pendingDetach) {
        continue;
      }
      distance += row.distance ?? 0;
      cost += row.deliveryCost ?? 0;
      loads++;
    }
    return {distance, cost, loads};
  }
}
