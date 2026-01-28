import { computed, inject } from "@angular/core";
import {
  Api,
  deleteEmployee,
  getDocuments,
  getEmployeeById,
  getLoads,
} from "@logistics/shared/api";
import type { DocumentDto, EmployeeDto, LoadDto } from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

interface EmployeeDetailsState {
  employeeId: string | null;
  employee: EmployeeDto | null;
  recentLoads: LoadDto[];
  documents: DocumentDto[];
  isLoading: boolean;
  isProcessing: boolean;
  error: string | null;
  editDialogVisible: boolean;
}

const initialState: EmployeeDetailsState = {
  employeeId: null,
  employee: null,
  recentLoads: [],
  documents: [],
  isLoading: false,
  isProcessing: false,
  error: null,
  editDialogVisible: false,
};

export const EmployeeDetailsStore = signalStore(
  withState(initialState),
  withComputed((store) => {
    const fullName = computed(() => store.employee()?.fullName ?? "");
    const hasRole = computed(() => !!store.employee()?.role);
    const isActive = computed(() => store.employee()?.status === "active");
    const recentLoadsCount = computed(() => store.recentLoads().length);
    const documentsCount = computed(() => store.documents().length);

    return {
      fullName,
      hasRole,
      isActive,
      recentLoadsCount,
      documentsCount,
    };
  }),
  withMethods((store, api = inject(Api)) => ({
    reset() {
      patchState(store, initialState);
    },

    async loadEmployee(employeeId: string) {
      patchState(store, { employeeId, isLoading: true, error: null });

      try {
        const [employee, loadsResponse, documentsResponse] = await Promise.all([
          api.invoke(getEmployeeById, { userId: employeeId }),
          api.invoke(getLoads, {
            UserId: employeeId,
            PageSize: 10,
            OrderBy: "-DeliveryDate",
          }),
          api.invoke(getDocuments, {
            OwnerId: employeeId,
            OwnerType: "employee",
          }),
        ]);

        patchState(store, {
          employee,
          recentLoads: loadsResponse?.items ?? [],
          documents: documentsResponse ?? [],
          isLoading: false,
        });
      } catch (error) {
        patchState(store, {
          error: "Failed to load employee details",
          isLoading: false,
        });
        console.error("Failed to load employee:", error);
      }
    },

    async refreshEmployee() {
      const employeeId = store.employeeId();
      if (!employeeId) return;

      try {
        const employee = await api.invoke(getEmployeeById, { userId: employeeId });
        patchState(store, { employee });
      } catch (error) {
        console.error("Failed to refresh employee:", error);
      }
    },

    async refreshLoads() {
      const employeeId = store.employeeId();
      if (!employeeId) return;

      try {
        const loadsResponse = await api.invoke(getLoads, {
          UserId: employeeId,
          PageSize: 10,
          OrderBy: "-DeliveryDate",
        });
        patchState(store, { recentLoads: loadsResponse?.items ?? [] });
      } catch (error) {
        console.error("Failed to refresh loads:", error);
      }
    },

    async refreshDocuments() {
      const employeeId = store.employeeId();
      if (!employeeId) return;

      try {
        const documentsResponse = await api.invoke(getDocuments, {
          OwnerId: employeeId,
          OwnerType: "employee",
        });
        patchState(store, { documents: documentsResponse ?? [] });
      } catch (error) {
        console.error("Failed to refresh documents:", error);
      }
    },

    async deleteEmployee(): Promise<boolean> {
      const employeeId = store.employeeId();
      if (!employeeId) return false;

      patchState(store, { isProcessing: true, error: null });

      try {
        await api.invoke(deleteEmployee, { userId: employeeId });
        patchState(store, { isProcessing: false });
        return true;
      } catch (error) {
        patchState(store, {
          error: "Failed to delete employee",
          isProcessing: false,
        });
        console.error("Failed to delete employee:", error);
        return false;
      }
    },

    openEditDialog() {
      patchState(store, { editDialogVisible: true });
    },

    closeEditDialog() {
      patchState(store, { editDialogVisible: false });
    },
  })),
);
