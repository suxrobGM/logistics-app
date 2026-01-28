import { computed, inject } from "@angular/core";
import {
  Api,
  deleteCustomer,
  getCustomerById,
  getCustomerUsersByCustomer,
  getInvoices,
  getLoads,
} from "@logistics/shared/api";
import type { CustomerDto, CustomerUserDto, InvoiceDto, LoadDto } from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

interface CustomerDetailsState {
  customerId: string | null;
  customer: CustomerDto | null;
  invoices: InvoiceDto[];
  loads: LoadDto[];
  portalUsers: CustomerUserDto[];
  isLoading: boolean;
  isProcessing: boolean;
  error: string | null;
  editDialogVisible: boolean;
}

const initialState: CustomerDetailsState = {
  customerId: null,
  customer: null,
  invoices: [],
  loads: [],
  portalUsers: [],
  isLoading: false,
  isProcessing: false,
  error: null,
  editDialogVisible: false,
};

export const CustomerDetailsStore = signalStore(
  withState(initialState),
  withComputed((store) => {
    const name = computed(() => store.customer()?.name ?? "");
    const isActive = computed(() => store.customer()?.status === "active");
    const invoicesCount = computed(() => store.invoices().length);
    const loadsCount = computed(() => store.loads().length);
    const portalUsersCount = computed(() => store.portalUsers().length);

    return {
      name,
      isActive,
      invoicesCount,
      loadsCount,
      portalUsersCount,
    };
  }),
  withMethods((store, api = inject(Api)) => ({
    reset() {
      patchState(store, initialState);
    },

    async loadCustomer(customerId: string) {
      patchState(store, { customerId, isLoading: true, error: null });

      try {
        const [customer, invoicesResponse, loadsResponse, portalUsers] = await Promise.all([
          api.invoke(getCustomerById, { id: customerId }),
          api.invoke(getInvoices, {
            CustomerId: customerId,
            PageSize: 10,
            OrderBy: "-DueDate",
          }),
          api.invoke(getLoads, {
            CustomerId: customerId,
            PageSize: 10,
            OrderBy: "-DeliveryDate",
          }),
          api.invoke(getCustomerUsersByCustomer, { customerId }),
        ]);

        patchState(store, {
          customer,
          invoices: invoicesResponse?.items ?? [],
          loads: loadsResponse?.items ?? [],
          portalUsers: portalUsers ?? [],
          isLoading: false,
        });
      } catch (error) {
        patchState(store, {
          error: "Failed to load customer details",
          isLoading: false,
        });
        console.error("Failed to load customer:", error);
      }
    },

    async refreshCustomer() {
      const customerId = store.customerId();
      if (!customerId) return;

      try {
        const customer = await api.invoke(getCustomerById, { id: customerId });
        patchState(store, { customer });
      } catch (error) {
        console.error("Failed to refresh customer:", error);
      }
    },

    async refreshInvoices() {
      const customerId = store.customerId();
      if (!customerId) return;

      try {
        const invoicesResponse = await api.invoke(getInvoices, {
          CustomerId: customerId,
          PageSize: 10,
          OrderBy: "-DueDate",
        });
        patchState(store, { invoices: invoicesResponse?.items ?? [] });
      } catch (error) {
        console.error("Failed to refresh invoices:", error);
      }
    },

    async refreshLoads() {
      const customerId = store.customerId();
      if (!customerId) return;

      try {
        const loadsResponse = await api.invoke(getLoads, {
          CustomerId: customerId,
          PageSize: 10,
          OrderBy: "-DeliveryDate",
        });
        patchState(store, { loads: loadsResponse?.items ?? [] });
      } catch (error) {
        console.error("Failed to refresh loads:", error);
      }
    },

    async refreshPortalUsers() {
      const customerId = store.customerId();
      if (!customerId) return;

      try {
        const portalUsers = await api.invoke(getCustomerUsersByCustomer, { customerId });
        patchState(store, { portalUsers: portalUsers ?? [] });
      } catch (error) {
        console.error("Failed to refresh portal users:", error);
      }
    },

    async deleteCustomer(): Promise<boolean> {
      const customerId = store.customerId();
      if (!customerId) return false;

      patchState(store, { isProcessing: true, error: null });

      try {
        await api.invoke(deleteCustomer, { id: customerId });
        patchState(store, { isProcessing: false });
        return true;
      } catch (error) {
        patchState(store, {
          error: "Failed to delete customer",
          isProcessing: false,
        });
        console.error("Failed to delete customer:", error);
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
