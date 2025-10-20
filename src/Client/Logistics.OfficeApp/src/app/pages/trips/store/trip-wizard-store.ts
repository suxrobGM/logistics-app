import { computed, inject } from "@angular/core";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { pipe, switchMap, tap } from "rxjs";
import { ApiService } from "@/core/api";
import {
  CreateTripLoadCommand,
  LoadStatus,
  OptimizeTripStopsCommand,
  TripLoadDto,
  TripStopDto,
  TripStopType,
} from "@/core/api/models";

// Internal types for store state
interface NewLoad extends CreateTripLoadCommand {
  id: string;
  tempId: string;
}

export interface TableRow extends TripLoadDto {
  kind: "existing" | "new";
  pendingDetach?: boolean;
}

// Store state interface for the Trip Wizard component
interface TripWizardState {
  // Step 1 - Basic Info
  tripName: string;
  truckId: string;
  truckVehicleCapacity: number;

  // Step 2 - Loads
  newLoads: NewLoad[];
  attachedLoads: TripLoadDto[];
  detachedLoads: TripLoadDto[];

  // Step 3 - Review & Optimization
  stops: TripStopDto[];
  totalDistance: number;
  totalCost: number;
  stopsNeedRegeneration: boolean; // Flag to track if stops need to be regenerated

  // UI State
  activeStep: number;
  isOptimizing: boolean;
  selectedStop: TripStopDto | null;

  // Edit mode support
  mode: "create" | "edit";
  initialLoads: TripLoadDto[];
  initialStops: TripStopDto[];
}

const initialState: TripWizardState = {
  // Step 1
  tripName: "",
  truckId: "",
  truckVehicleCapacity: 0,

  // Step 2
  newLoads: [],
  attachedLoads: [],
  detachedLoads: [],

  // Step 3
  stops: [],
  totalDistance: 0,
  totalCost: 0,
  stopsNeedRegeneration: true,

  // UI
  activeStep: 1,
  isOptimizing: false,
  selectedStop: null,

  // Edit mode
  mode: "create",
  initialLoads: [],
  initialStops: [],
};

interface TripWizardInitializeData {
  mode: "create" | "edit";
  tripName?: string;
  truckId?: string;
  truckVehicleCapacity?: number;
  loads?: TripLoadDto[];
  stops?: TripStopDto[];
  totalDistance?: number;
  totalCost?: number;
}

export const TripWizardStore = signalStore(
  withState(initialState),
  withComputed((store) => {
    // Computed table rows for Step 2
    const tableRows = computed<TableRow[]>(() => {
      const existingRows: TableRow[] = store.attachedLoads().map((load) => ({
        ...load,
        kind: "existing" as const,
        pendingDetach: store.detachedLoads().some((d) => d.id === load.id),
      }));

      const newRows: TableRow[] = store.newLoads().map((load) => ({
        ...load,
        kind: "new" as const,
        number: 0,
        status: LoadStatus.Draft,
        pendingDetach: false,
      }));

      return [...existingRows, ...newRows];
    });

    // Active loads (excluding pending detach)
    const activeLoads = computed(() => {
      const detachedIds = new Set(store.detachedLoads().map((l: TripLoadDto) => l.id));
      return tableRows().filter((row: TableRow) => !detachedIds.has(row.id));
    });

    // Total loads count (active only)
    const totalLoads = computed(() => activeLoads().length);

    // New loads count
    const newLoadsCount = computed(() => store.newLoads().length);

    // Pending detach count
    const pendingDetachLoadsCount = computed(() => store.detachedLoads().length);

    // Total distance from active loads
    const totalDistanceFromLoads = computed(() => {
      return activeLoads().reduce(
        (total: number, load: TableRow) => total + (load.distance ?? 0),
        0,
      );
    });

    // Total cost from active loads
    const totalCostFromLoads = computed(() => {
      return activeLoads().reduce(
        (total: number, load: TableRow) => total + (load.deliveryCost ?? 0),
        0,
      );
    });

    // Review data for Step 3
    const reviewData = computed(() => ({
      tripName: store.tripName(),
      truckId: store.truckId(),
      totalLoads: totalLoads(),
      newLoadsCount: newLoadsCount(),
      pendingDetachLoadsCount: pendingDetachLoadsCount(),
      totalDistance: store.totalDistance(),
      totalCost: store.totalCost(),
      truckVehicleCapacity: store.truckVehicleCapacity(),
      stops: store.stops(),
    }));

    // Final wizard value for submission
    const wizardValue = computed(() => ({
      tripName: store.tripName(),
      truckId: store.truckId(),
      truckVehicleCapacity: store.truckVehicleCapacity(),
      newLoads: store.newLoads().length > 0 ? store.newLoads() : null,
      attachedLoads: store.attachedLoads().length > 0 ? store.attachedLoads() : null,
      detachedLoads: store.detachedLoads().length > 0 ? store.detachedLoads() : null,
      stops: store.stops(),
      totalDistance: store.totalDistance(),
      totalCost: store.totalCost(),
      totalLoads: totalLoads(),
      initialLoads: store.initialLoads(),
      initialStops: store.initialStops(),
    }));

    // Check if we can proceed to next step
    const canProceedFromStep1 = computed(() => {
      return store.tripName().trim() !== "" && store.truckId() !== "";
    });

    const canProceedFromStep2 = computed(() => {
      return totalLoads() > 0;
    });

    return {
      tableRows,
      activeLoads,
      totalLoads,
      newLoadsCount,
      pendingDetachLoadsCount,
      totalDistanceFromLoads,
      totalCostFromLoads,
      reviewData,
      wizardValue,
      canProceedFromStep1,
      canProceedFromStep2,
    };
  }),
  withMethods((store, apiService = inject(ApiService)) => ({
    // Reset store to initial state
    reset() {
      patchState(store, initialState);
    },

    // Initialize wizard (for edit mode)
    initialize(data: TripWizardInitializeData) {
      patchState(store, {
        mode: data.mode,
        tripName: data.tripName ?? "",
        truckId: data.truckId ?? "",
        truckVehicleCapacity: data.truckVehicleCapacity ?? 0,
        attachedLoads: data.loads ?? [],
        stops: data.stops ?? [],
        totalDistance: data.totalDistance ?? 0,
        totalCost: data.totalCost ?? 0,
        initialLoads: data.loads ?? [],
        initialStops: data.stops ?? [],
        newLoads: [],
        detachedLoads: [],
        activeStep: 1,
        stopsNeedRegeneration: data.stops && data.stops.length > 0 ? false : true, // If we have initial stops, they're fresh
      });
    },

    // Step 1 Methods
    setBasicInfo(data: { tripName: string; truckId: string; truckVehicleCapacity: number }): void {
      patchState(store, {
        tripName: data.tripName,
        truckId: data.truckId,
        truckVehicleCapacity: data.truckVehicleCapacity,
      });
    },

    // Step 2 Methods - Load Management
    addNewLoad(load: CreateTripLoadCommand): void {
      const tempId = crypto.randomUUID();
      const newLoad: NewLoad = {
        ...load,
        id: tempId,
        tempId: tempId,
      };

      patchState(store, {
        newLoads: [...store.newLoads(), newLoad],
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    removeNewLoad(loadId: string): void {
      patchState(store, {
        newLoads: store.newLoads().filter((l) => l.id !== loadId),
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    attachExistingLoad(load: TripLoadDto): void {
      patchState(store, {
        attachedLoads: [...store.attachedLoads(), load],
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    detachLoad(loadId: string): void {
      const load = store.tableRows().find((l) => l.id === loadId);
      if (!load) return;

      // Check if already detached
      if (store.detachedLoads().some((l) => l.id === loadId)) return;

      patchState(store, {
        detachedLoads: [...store.detachedLoads(), load as TripLoadDto],
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    undoDetachLoad(loadId: string): void {
      patchState(store, {
        detachedLoads: store.detachedLoads().filter((l) => l.id !== loadId),
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    // Step 3 Methods
    setSelectedStop(stop: TripStopDto | null): void {
      patchState(store, { selectedStop: stop });
    },

    updateStops(stops: TripStopDto[]): void {
      patchState(store, { stops });
    },

    // Navigation
    goToStep(step: number): void {
      patchState(store, { activeStep: step });
    },

    nextStep(): void {
      patchState(store, { activeStep: store.activeStep() + 1 });
    },

    previousStep(): void {
      patchState(store, { activeStep: store.activeStep() - 1 });
    },

    // Optimization - Step 2 (initial optimization)
    optimizeStopsFromStep2: rxMethod<void>(
      pipe(
        switchMap(() => {
          patchState(store, { isOptimizing: true });

          const activeLoads = store.activeLoads();
          const totalDistFromLoads = store.totalDistanceFromLoads();
          const totalCostFromLoads = store.totalCostFromLoads();
          const stops = buildStopsFromLoads(activeLoads);

          const command: OptimizeTripStopsCommand = {
            maxVehicles: store.truckVehicleCapacity(),
            stops: stops,
          };

          return apiService.tripApi.optimizeTripStops(command).pipe(
            tap({
              next: (result: { data?: { orderedStops: TripStopDto[]; totalDistance: number } }) => {
                patchState(store, {
                  stops: result.data?.orderedStops ?? stops,
                  totalDistance: result.data?.totalDistance ?? totalDistFromLoads,
                  totalCost: totalCostFromLoads,
                  isOptimizing: false,
                  stopsNeedRegeneration: false, // Stops are now fresh
                });
              },
              error: (error: unknown) => {
                console.error("Failed to optimize stops:", error);
                // Fallback to non-optimized stops
                patchState(store, {
                  stops: stops,
                  totalDistance: totalDistFromLoads,
                  totalCost: totalCostFromLoads,
                  isOptimizing: false,
                  stopsNeedRegeneration: false, // Stops are now fresh (even if not optimized)
                });
              },
            }),
          );
        }),
      ),
    ),

    // Re-optimization - Step 3
    reOptimizeStops: rxMethod<void>(
      pipe(
        switchMap(() => {
          patchState(store, { isOptimizing: true });

          const command: OptimizeTripStopsCommand = {
            maxVehicles: store.truckVehicleCapacity(),
            stops: store.stops(),
          };

          return apiService.tripApi.optimizeTripStops(command).pipe(
            tap({
              next: (result: {
                success: boolean;
                data?: { orderedStops: TripStopDto[]; totalDistance: number };
              }) => {
                if (result.success && result.data) {
                  patchState(store, {
                    stops: result.data.orderedStops,
                    totalDistance: result.data.totalDistance,
                    isOptimizing: false,
                  });
                }
              },
              error: (error: unknown) => {
                console.error("Failed to re-optimize stops:", error);
                patchState(store, { isOptimizing: false });
              },
            }),
          );
        }),
      ),
    ),
  })),
);

// Helper function to build stops from active loads
function buildStopsFromLoads(loads: TableRow[]): TripStopDto[] {
  const stops: TripStopDto[] = [];
  let order = 1;

  for (const load of loads) {
    // Pick-up
    stops.push({
      id: crypto.randomUUID(),
      order: order++,
      type: TripStopType.PickUp,
      address: load.originAddress,
      location: {
        longitude: load.originLocation.longitude,
        latitude: load.originLocation.latitude,
      },
      loadId: load.id,
    });

    // Drop-off
    stops.push({
      id: crypto.randomUUID(),
      order: order++,
      type: TripStopType.DropOff,
      address: load.destinationAddress,
      location: {
        longitude: load.destinationLocation.longitude,
        latitude: load.destinationLocation.latitude,
      },
      loadId: load.id,
    });
  }

  return stops;
}
