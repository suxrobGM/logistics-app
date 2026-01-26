import { computed, inject } from "@angular/core";
import { isEmptyGuid } from "@logistics/shared";
import { Api, optimizeTripStops } from "@logistics/shared/api";
import type {
  CreateTripLoadCommand,
  OptimizeTripStopsCommand,
  TripLoadDto,
  TripStopDto,
} from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

// Internal types for store state
interface NewLoad extends CreateTripLoadCommand {
  id: string;
  tempId: string;
}

// TableRow type that supports both existing loads (with customer) and new loads (without customer)
export interface TableRow extends Omit<TripLoadDto, "customer"> {
  kind: "existing" | "new";
  pendingDetach?: boolean;
  customer?: TripLoadDto["customer"]; // Optional for new loads
  customerId?: string; // From CreateTripLoadCommand
}

// Store state interface for the Trip Wizard component
interface TripWizardState {
  // Step 1 - Basic Info
  tripName: string;
  truckId: string | null; // optional - trip can be created without truck assignment
  truckNumber: string | null; // Display-friendly truck number
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
  truckId: null,
  truckNumber: null,
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
  truckId?: string | null;
  truckNumber?: string | null;
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
        status: "draft",
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
      truckNumber: store.truckNumber(),
      totalLoads: totalLoads(),
      newLoadsCount: newLoadsCount(),
      pendingDetachLoadsCount: pendingDetachLoadsCount(),
      totalDistance: store.totalDistance(),
      // Use calculated cost from loads for consistency in both create and edit modes
      totalCost: totalCostFromLoads(),
      truckVehicleCapacity: store.truckVehicleCapacity(),
      stops: store.stops(),
    }));

    // Newly attached load IDs (loads added in create mode, not from initial edit data)
    const attachedLoadIds = computed<string[]>(() => {
      if (store.mode() === "edit") {
        // In edit mode, attached loads are the ones not in the initial loads
        const initialIds = new Set(store.initialLoads().map((l) => l.id));
        return store.attachedLoads()
          .filter((l) => l.id && !initialIds.has(l.id))
          .map((l) => l.id!)
          .filter((id): id is string => !!id);
      }
      // In create mode, all attached loads are newly added
      return store.attachedLoads()
        .map((l) => l.id)
        .filter((id): id is string => !!id);
    });

    // Final wizard value for submission
    const wizardValue = computed(() => ({
      tripName: store.tripName(),
      truckId: store.truckId(),
      truckVehicleCapacity: store.truckVehicleCapacity(),
      newLoads: store.newLoads().length > 0 ? store.newLoads() : null,
      attachedLoadIds: attachedLoadIds().length > 0 ? attachedLoadIds() : null,
      detachedLoads: store.detachedLoads().length > 0 ? store.detachedLoads() : null,
      stops: store.stops(),
      totalDistance: store.totalDistance(),
      totalCost: totalCostFromLoads(),
      totalLoads: totalLoads(),
      initialLoads: store.initialLoads(),
      initialStops: store.initialStops(),
    }));

    // Check if we can proceed to next step (truck is now optional)
    const canProceedFromStep1 = computed(() => {
      return store.tripName().trim() !== "";
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
      attachedLoadIds,
    };
  }),
  withMethods((store, api = inject(Api)) => ({
    // Reset store to initial state
    reset() {
      patchState(store, initialState);
    },

    // Initialize wizard (for edit mode)
    initialize(data: TripWizardInitializeData) {
      // Normalize empty GUID to null
      const normalizedTruckId = isEmptyGuid(data.truckId) ? null : data.truckId;

      patchState(store, {
        mode: data.mode,
        tripName: data.tripName ?? "",
        truckId: normalizedTruckId,
        truckNumber: data.truckNumber ?? null,
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
    setBasicInfo(data: {
      tripName: string;
      truckId: string | null;
      truckNumber: string | null;
      truckVehicleCapacity: number;
    }): void {
      patchState(store, {
        tripName: data.tripName,
        truckId: data.truckId,
        truckNumber: data.truckNumber,
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

    // Attach an existing unassigned load to this trip
    attachExistingLoad(load: TripLoadDto): void {
      // Check if already attached
      if (store.attachedLoads().some((l) => l.id === load.id)) return;

      patchState(store, {
        attachedLoads: [...store.attachedLoads(), load],
        stopsNeedRegeneration: true, // Mark stops as needing regeneration
      });
    },

    // Remove an attached existing load (not detach - used for undoing attach)
    removeAttachedLoad(loadId: string): void {
      patchState(store, {
        attachedLoads: store.attachedLoads().filter((l) => l.id !== loadId),
        stopsNeedRegeneration: true,
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
    async optimizeStopsFromStep2(): Promise<void> {
      patchState(store, { isOptimizing: true });

      const activeLoads = store.activeLoads();
      const totalCostFromLoads = store.totalCostFromLoads();
      const stops = buildStopsFromLoads(activeLoads);

      const command: OptimizeTripStopsCommand = {
        // Use truck capacity if assigned, otherwise default to 8 (typical car hauler capacity)
        maxVehicles: store.truckVehicleCapacity() || 8,
        stops: stops,
      };

      try {
        const result = await api.invoke(optimizeTripStops, { body: command });
        patchState(store, {
          stops: result?.orderedStops ?? stops,
          totalDistance: result?.totalDistance ?? 0,
          totalCost: totalCostFromLoads,
          isOptimizing: false,
          stopsNeedRegeneration: false,
        });
      } catch (error) {
        console.error("Failed to optimize stops:", error);
        // Fallback to non-optimized stops
        patchState(store, {
          stops: stops,
          totalDistance: 0,
          totalCost: totalCostFromLoads,
          isOptimizing: false,
          stopsNeedRegeneration: false,
        });
      }
    },

    // Re-optimization - Step 3
    async reOptimizeStops(): Promise<void> {
      patchState(store, { isOptimizing: true });

      const currentStops = store.stops();
      const command: OptimizeTripStopsCommand = {
        // Use truck capacity if assigned, otherwise default to 8 (typical car hauler capacity)
        maxVehicles: store.truckVehicleCapacity() || 8,
        stops: currentStops,
      };

      try {
        const result = await api.invoke(optimizeTripStops, { body: command });
        patchState(store, {
          stops: result?.orderedStops ?? currentStops,
          totalDistance: result?.totalDistance ?? store.totalDistance(),
          isOptimizing: false,
        });
      } catch (error) {
        console.error("Failed to re-optimize stops:", error);
        patchState(store, { isOptimizing: false });
      }
    },
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
      type: "pick_up",
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
      type: "drop_off",
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
