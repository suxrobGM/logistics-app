import { computed, inject } from "@angular/core";
import {
  Api,
  createLoadBoardProvider,
  deleteLoadBoardProvider,
  getLoadBoardProviders,
  getPostedTrucks,
  getTrucks,
  postTruckToLoadBoard,
  removePostedTruck,
  type CreateLoadBoardConfigurationCommand,
  type LoadBoardConfigurationDto,
  type LoadBoardProviderType,
  type PostedTruckDto,
  type PostTruckToLoadBoardCommand,
  type TruckDto,
} from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { ToastService } from "@/core/services";
import { getProviderLabel } from "../_components/loadboard.constants";

interface LoadBoardState {
  providers: LoadBoardConfigurationDto[];
  postedTrucks: PostedTruckDto[];
  trucks: TruckDto[];
  loaded: boolean;
  loading: boolean;
  error: string | null;
}

const initialState: LoadBoardState = {
  providers: [],
  postedTrucks: [],
  trucks: [],
  loaded: false,
  loading: false,
  error: null,
};

export const LoadBoardStore = signalStore(
  { providedIn: "root" },
  withState(initialState),
  withComputed((store) => ({
    activeProviders: computed(() => store.providers().filter((p) => p.isActive)),
    hasProviders: computed(() => store.providers().length > 0),
    hasActiveProviders: computed(() => store.providers().some((p) => p.isActive)),
    activePostedTrucksCount: computed(
      () => store.postedTrucks().filter((t) => t.status === "available").length,
    ),
    activeProviderOptions: computed(() =>
      store
        .providers()
        .filter((p) => p.isActive)
        .map((p) => ({
          label: getProviderLabel(p.providerType),
          value: p.providerType as LoadBoardProviderType,
        })),
    ),
  })),
  withMethods((store, api = inject(Api), toast = inject(ToastService)) => {
    async function refreshProviders(): Promise<void> {
      const data = await api.invoke(getLoadBoardProviders);
      patchState(store, { providers: data ?? [] });
    }

    async function refreshPostedTrucks(): Promise<void> {
      const data = await api.invoke(getPostedTrucks, {});
      patchState(store, { postedTrucks: data ?? [] });
    }

    async function refreshTrucks(): Promise<void> {
      const data = await api.invoke(getTrucks, {});
      patchState(store, { trucks: data?.items ?? [] });
    }

    return {
      async loadAll(force = false): Promise<void> {
        if (store.loaded() && !force) return;
        patchState(store, { loading: true, error: null });
        try {
          await Promise.all([refreshProviders(), refreshPostedTrucks(), refreshTrucks()]);
          patchState(store, { loaded: true });
        } catch (err) {
          console.error("Failed to load load board data:", err);
          patchState(store, { error: "Failed to load data" });
        } finally {
          patchState(store, { loading: false });
        }
      },

      async loadProviders(): Promise<void> {
        patchState(store, { loading: true, error: null });
        try {
          await refreshProviders();
        } catch (err) {
          console.error("Failed to load providers:", err);
          patchState(store, { error: "Failed to load load board providers" });
        } finally {
          patchState(store, { loading: false });
        }
      },

      async loadPostedTrucks(): Promise<void> {
        patchState(store, { loading: true, error: null });
        try {
          await Promise.all([refreshPostedTrucks(), refreshProviders(), refreshTrucks()]);
        } catch (err) {
          console.error("Failed to load posted trucks:", err);
          patchState(store, { error: "Failed to load data" });
        } finally {
          patchState(store, { loading: false });
        }
      },

      async createProvider(body: CreateLoadBoardConfigurationCommand): Promise<boolean> {
        try {
          await api.invoke(createLoadBoardProvider, { body });
          await refreshProviders();
          toast.showSuccess("Provider added successfully");
          return true;
        } catch (err) {
          console.error("Error saving provider:", err);
          toast.showError("Failed to add provider");
          return false;
        }
      },

      async deleteProvider(providerId: string): Promise<void> {
        try {
          await api.invoke(deleteLoadBoardProvider, { providerId });
          await refreshProviders();
          toast.showSuccess("Provider deleted successfully");
        } catch (err) {
          console.error("Error deleting provider:", err);
          toast.showError("Failed to delete provider");
        }
      },

      async postTruck(body: PostTruckToLoadBoardCommand): Promise<boolean> {
        try {
          await api.invoke(postTruckToLoadBoard, { body });
          await refreshPostedTrucks();
          toast.showSuccess("Truck posted successfully");
          return true;
        } catch (err) {
          console.error("Error posting truck:", err);
          toast.showError("Failed to post truck");
          return false;
        }
      },

      async removePostedTruck(postedTruckId: string): Promise<void> {
        try {
          await api.invoke(removePostedTruck, { postedTruckId });
          await refreshPostedTrucks();
          toast.showSuccess("Truck post removed successfully");
        } catch (err) {
          console.error("Error removing post:", err);
          toast.showError("Failed to remove truck post");
        }
      },
    };
  }),
);
