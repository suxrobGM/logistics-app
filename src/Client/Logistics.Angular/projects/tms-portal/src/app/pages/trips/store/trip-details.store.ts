import { computed, inject } from "@angular/core";
import {
  Api,
  getTripById,
  getTripTimeline,
  dispatchTrip,
  cancelTrip,
  markStopArrived,
} from "@logistics/shared/api";
import type {
  TripDto,
  TripStopDto,
  TripTimelineDto,
  TripTimelineEventDto,
} from "@logistics/shared/api";
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from "@ngrx/signals";

interface TripDetailsState {
  tripId: string | null;
  trip: TripDto | null;
  timeline: TripTimelineDto | null;
  selectedStop: TripStopDto | null;
  isLoading: boolean;
  isProcessing: boolean;
  error: string | null;
}

const initialState: TripDetailsState = {
  tripId: null,
  trip: null,
  timeline: null,
  selectedStop: null,
  isLoading: false,
  isProcessing: false,
  error: null,
};

export const TripDetailsStore = signalStore(
  withState(initialState),
  withComputed((store) => {
    // Sorted stops by order
    const sortedStops = computed<TripStopDto[]>(() => {
      return (store.trip()?.stops ?? []).slice().sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
    });

    // Pending stops (not yet arrived)
    const pendingStops = computed<TripStopDto[]>(() => {
      return sortedStops().filter((stop) => !stop.arrivedAt);
    });

    // Completed stops (arrived)
    const completedStops = computed<TripStopDto[]>(() => {
      return sortedStops().filter((stop) => !!stop.arrivedAt);
    });

    // Progress percentage
    const progressPercentage = computed<number>(() => {
      const total = sortedStops().length;
      if (total === 0) return 0;
      return Math.round((completedStops().length / total) * 100);
    });

    // Timeline events sorted by timestamp
    const timelineEvents = computed<TripTimelineEventDto[]>(() => {
      return (store.timeline()?.events ?? []).slice().sort(
        (a, b) => new Date(a.timestamp ?? 0).getTime() - new Date(b.timestamp ?? 0).getTime()
      );
    });

    // Check if trip can be dispatched (Draft status with truck assigned)
    const canDispatch = computed<boolean>(() => {
      const trip = store.trip();
      return trip?.status === "draft" && !!trip?.truckId;
    });

    // Check if trip can be cancelled (not completed or already cancelled)
    const canCancel = computed<boolean>(() => {
      const status = store.trip()?.status;
      return status === "draft" || status === "dispatched" || status === "in_transit";
    });

    // Check if trip is active (can mark stops as arrived)
    const isActive = computed<boolean>(() => {
      const status = store.trip()?.status;
      return status === "dispatched" || status === "in_transit";
    });

    // Check if trip is read-only (completed or cancelled)
    const isReadOnly = computed<boolean>(() => {
      const status = store.trip()?.status;
      return status === "completed" || status === "cancelled";
    });

    return {
      sortedStops,
      pendingStops,
      completedStops,
      progressPercentage,
      timelineEvents,
      canDispatch,
      canCancel,
      isActive,
      isReadOnly,
    };
  }),
  withMethods((store, api = inject(Api)) => ({
    // Reset store
    reset() {
      patchState(store, initialState);
    },

    // Load trip data
    async loadTrip(tripId: string) {
      patchState(store, { tripId, isLoading: true, error: null });

      try {
        const [trip, timeline] = await Promise.all([
          api.invoke(getTripById, { tripId }),
          api.invoke(getTripTimeline, { tripId }),
        ]);

        patchState(store, {
          trip,
          timeline,
          isLoading: false,
        });
      } catch (error) {
        patchState(store, {
          error: "Failed to load trip details",
          isLoading: false,
        });
        console.error("Failed to load trip:", error);
      }
    },

    // Refresh trip data
    async refreshTrip() {
      const tripId = store.tripId();
      if (!tripId) return;

      try {
        const [trip, timeline] = await Promise.all([
          api.invoke(getTripById, { tripId }),
          api.invoke(getTripTimeline, { tripId }),
        ]);

        patchState(store, { trip, timeline });
      } catch (error) {
        console.error("Failed to refresh trip:", error);
      }
    },

    // Dispatch trip
    async dispatchTrip(): Promise<boolean> {
      const tripId = store.tripId();
      if (!tripId) return false;

      patchState(store, { isProcessing: true, error: null });

      try {
        await api.invoke(dispatchTrip, { tripId });

        // Refresh trip data after dispatch
        await this.refreshTrip();

        patchState(store, { isProcessing: false });
        return true;
      } catch (error) {
        patchState(store, {
          error: "Failed to dispatch trip",
          isProcessing: false,
        });
        console.error("Failed to dispatch trip:", error);
        return false;
      }
    },

    // Cancel trip
    async cancelTrip(reason?: string): Promise<boolean> {
      const tripId = store.tripId();
      if (!tripId) return false;

      patchState(store, { isProcessing: true, error: null });

      try {
        await api.invoke(cancelTrip, {
          tripId,
          body: { tripId, reason },
        });

        // Refresh trip data after cancel
        await this.refreshTrip();

        patchState(store, { isProcessing: false });
        return true;
      } catch (error) {
        patchState(store, {
          error: "Failed to cancel trip",
          isProcessing: false,
        });
        console.error("Failed to cancel trip:", error);
        return false;
      }
    },

    // Mark stop as arrived
    async markStopArrived(stopId: string): Promise<boolean> {
      const tripId = store.tripId();
      if (!tripId) return false;

      patchState(store, { isProcessing: true, error: null });

      try {
        await api.invoke(markStopArrived, { tripId, stopId });

        // Refresh trip data after marking stop
        await this.refreshTrip();

        patchState(store, { isProcessing: false });
        return true;
      } catch (error) {
        patchState(store, {
          error: "Failed to mark stop as arrived",
          isProcessing: false,
        });
        console.error("Failed to mark stop as arrived:", error);
        return false;
      }
    },

    // Select a stop
    selectStop(stop: TripStopDto | null) {
      patchState(store, { selectedStop: stop });
    },
  }))
);
