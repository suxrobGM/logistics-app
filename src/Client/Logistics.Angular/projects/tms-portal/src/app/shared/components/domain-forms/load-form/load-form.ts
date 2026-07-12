import {
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  type OnInit,
} from "@angular/core";
import {
  disabled,
  form,
  FormField,
  FormRoot,
  maxLength,
  pattern,
  required,
} from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import {
  type Address,
  type ContainerDto,
  type CustomerDto,
  type GeoPoint,
  type HazmatClass,
  type LoadSource,
  type LoadStatus,
  type LoadType,
  type TerminalDto,
  type TruckDto,
} from "@logistics/shared/api";
import {
  hazmatClassOptions,
  loadSourceOptions,
  loadStatusOptions,
  loadTypeOptions,
} from "@logistics/shared/api/enums";
import {
  Grid,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiCheckboxField,
  UiCollapsible,
  UiDateField,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import {
  AddressAutocomplete,
  DirectionMap,
  type RouteChangeEvent,
  type SelectedAddressEvent,
  type Waypoint,
} from "@/shared/components/maps";
import {
  SearchContainer,
  SearchCustomer,
  SearchTerminal,
  SearchTruck,
} from "@/shared/components/search";
import { Converters } from "@/shared/utils";

/**
 * Form value interface for the Load Form.
 */
export interface LoadFormValue {
  name: string;
  type: LoadType;
  source: LoadSource;
  customer: CustomerDto;
  originAddress: Address;
  originLocation: GeoPoint;
  destinationAddress: Address;
  destinationLocation: GeoPoint;
  deliveryCost: number;
  distance: number; // miles, read-only for users
  status?: LoadStatus | null; // only present in edit-mode
  assignedTruckId?: string | null; // optional - load can be created without truck assignment
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  tripId?: string | null;
  tripNumber?: number | null;
  // Schedule
  requestedPickupDate?: string | null;
  requestedDeliveryDate?: string | null;
  // Intermodal IDs (emitted from submit)
  containerId?: string | null;
  originTerminalId?: string | null;
  destinationTerminalId?: string | null;
  // Intermodal DTOs (for patching from edit-mode initial data)
  container?: ContainerDto | null;
  originTerminal?: TerminalDto | null;
  destinationTerminal?: TerminalDto | null;
  // Notes
  notes?: string | null;
  // Hazmat / ADR
  isHazmat?: boolean;
  hazmatClass?: HazmatClass | null;
  unNumber?: string | null;
}

const DUMMY_LOCATION: GeoPoint = { longitude: 0, latitude: 0 };

/**
 * The signal-form model. Field types mirror the `ui-*-field` wrappers they bind to (e.g.
 * `ui-select-field` is a `FormValueControl<T | null>`), so nullability here is a binding
 * requirement, not a validation one — `required()` in the schema enforces presence.
 */
interface LoadFormModel {
  name: string;
  type: LoadType | null;
  source: LoadSource | null;
  customer: CustomerDto | null;
  originAddress: Address | null;
  originLocation: GeoPoint;
  destinationAddress: Address | null;
  destinationLocation: GeoPoint;
  deliveryCost: number | null;
  distance: number | null;
  requestedPickupDate: Date | null;
  requestedDeliveryDate: Date | null;
  container: ContainerDto | null;
  originTerminal: TerminalDto | null;
  destinationTerminal: TerminalDto | null;
  notes: string;
  isHazmat: boolean;
  hazmatClass: HazmatClass | null;
  unNumber: string;
  status: LoadStatus | null;
  assignedTruck: TruckDto | null;
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  tripId: string | null;
  tripNumber: number | null;
}

const EMPTY: LoadFormModel = {
  name: "",
  type: "general_freight",
  source: "manual",
  customer: null,
  originAddress: null,
  originLocation: DUMMY_LOCATION,
  destinationAddress: null,
  destinationLocation: DUMMY_LOCATION,
  deliveryCost: 0,
  distance: 0,
  requestedPickupDate: null,
  requestedDeliveryDate: null,
  container: null,
  originTerminal: null,
  destinationTerminal: null,
  notes: "",
  isHazmat: false,
  hazmatClass: null,
  unNumber: "",
  status: null,
  assignedTruck: null,
  assignedDispatcherId: "",
  assignedDispatcherName: "",
  tripId: null,
  tripNumber: null,
};

@Component({
  selector: "app-load-form",
  templateUrl: "./load-form.html",
  imports: [
    AddressAutocomplete,
    DirectionMap,
    FormField,
    FormRoot,
    Grid,
    Icon,
    RouterLink,
    SearchContainer,
    SearchCustomer,
    SearchTerminal,
    SearchTruck,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiCheckboxField,
    UiCollapsible,
    UiDateField,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class LoadForm implements OnInit {
  protected readonly loadTypes = loadTypeOptions;
  protected readonly loadStatuses = loadStatusOptions;
  protected readonly loadSources = loadSourceOptions;
  protected readonly hazmatClasses = hazmatClassOptions;

  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  protected readonly originCoords = signal<Waypoint>({
    id: "origin",
    location: { longitude: 0, latitude: 0 },
  });
  protected readonly destinationCoords = signal<Waypoint>({
    id: "destination",
    location: { longitude: 0, latitude: 0 },
  });

  public readonly mode = input.required<"create" | "edit">();
  public readonly canChangeAssignedTruck = input<boolean>(true);
  public readonly initial = input<Partial<LoadFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly mapHeight = input<string>("600px");

  public readonly save = output<LoadFormValue>();
  public readonly remove = output<void>();

  protected readonly model = signal<LoadFormModel>({ ...EMPTY });

  /**
   * `distance`, `assignedDispatcherName`, `tripId` and `tripNumber` are permanently disabled — they
   * are computed or informational. `model()` still holds them (a disabled field drops out of
   * validation, not out of the value), so they ride along to the parent's save.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Load name is required." });
      required(p.type, { message: "Load type is required." });
      required(p.source, { message: "Load source is required." });
      required(p.customer, { message: "Customer is required." });
      required(p.originAddress, { message: "Origin address is required." });
      required(p.destinationAddress, { message: "Destination address is required." });
      required(p.deliveryCost, { message: "Delivery cost is required." });
      required(p.assignedDispatcherId, { message: "An assigned dispatcher is required." });

      maxLength(p.notes, 2000, { message: "Notes cannot exceed 2000 characters." });
      maxLength(p.unNumber, 16, { message: "UN number cannot exceed 16 characters." });
      pattern(p.unNumber, /^UN\d{4}$/i, {
        when: ({ valueOf }) => valueOf(p.unNumber).length > 0,
        message: "Enter a UN number such as UN1203.",
      });

      disabled(p.distance, { when: () => true });
      disabled(p.assignedDispatcherName, { when: () => true });
      disabled(p.tripId, { when: () => true });
      disabled(p.tripNumber, { when: () => true });
      disabled(p.assignedTruck, { when: () => !this.canChangeAssignedTruck() });
    },
    {
      submission: {
        action: async () => {
          this.save.emit(this.toFormValue());
          return undefined;
        },
      },
    },
  );

  /** `tripNumber` is a disabled, informational number rendered by a string-valued text field. */
  protected readonly tripNumberText = computed(() => this.model().tripNumber?.toString() ?? "");

  /** An edit-mode load carries only the truck's ID; `app-search-truck` resolves it to the DTO. */
  protected readonly initialTruckId = computed(() => this.initial()?.assignedTruckId ?? null);

  constructor() {
    effect(() => {
      const initialData = this.initial();
      if (!initialData) {
        return;
      }

      // Prevent overwriting user changes if the form is dirty in edit-mode
      if (this.mode() === "edit" && this.form().dirty()) {
        return;
      }

      this.patch(initialData);
    });
  }

  ngOnInit(): void {
    if (this.mode() === "create") {
      this.setCurrentDispatcher();
    }
  }

  protected updateOrigin(e: SelectedAddressEvent): void {
    const location = { longitude: e.center[0], latitude: e.center[1] };
    this.originCoords.set({ id: "origin", location });
    this.model.update((v) => ({ ...v, originLocation: location }));
  }

  protected updateDestination(e: SelectedAddressEvent): void {
    const location = { longitude: e.center[0], latitude: e.center[1] };
    this.destinationCoords.set({ id: "destination", location });
    this.model.update((v) => ({ ...v, destinationLocation: location }));
  }

  protected updateDistance(e: RouteChangeEvent): void {
    const miles = Converters.metersTo(e.distance, "mi");
    this.model.update((v) => ({ ...v, distance: miles }));
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this load?",
      accept: () => this.remove.emit(),
    });
  }

  private toFormValue(): LoadFormValue {
    const v = this.model();

    return {
      ...v,
      distance: Converters.toMeters(v.distance ?? 0, "mi"),
      assignedTruckId: v.assignedTruck?.id ?? null,
      requestedPickupDate: v.requestedPickupDate ? v.requestedPickupDate.toISOString() : null,
      requestedDeliveryDate: v.requestedDeliveryDate ? v.requestedDeliveryDate.toISOString() : null,
      containerId: v.container?.id ?? null,
      originTerminalId: v.originTerminal?.id ?? null,
      destinationTerminalId: v.destinationTerminal?.id ?? null,
      notes: v.notes || null,
      isHazmat: v.isHazmat,
      hazmatClass: v.isHazmat ? (v.hazmatClass ?? null) : null,
      unNumber: v.isHazmat ? v.unNumber || null : null,
    } as LoadFormValue;
  }

  private patch(src: Partial<LoadFormValue>): void {
    // Assign field by field rather than spreading `src`: `LoadFormValue` carries keys the model does
    // not have (`assignedTruckId`, `containerId`, `originTerminalId`, ...). `patchValue()` used to
    // drop them; a `model.update()` spread would add them to the form tree.
    // `assignedTruck` is deliberately absent — the form only ever receives an `assignedTruckId`, and
    // `app-search-truck` resolves that to the DTO through its `truckId` input.
    this.model.update((v) => ({
      ...v,
      name: src.name ?? v.name,
      type: src.type ?? v.type,
      source: src.source ?? v.source,
      customer: src.customer ?? v.customer,
      originAddress: src.originAddress ?? v.originAddress,
      originLocation: src.originLocation ?? v.originLocation,
      destinationAddress: src.destinationAddress ?? v.destinationAddress,
      destinationLocation: src.destinationLocation ?? v.destinationLocation,
      deliveryCost: src.deliveryCost ?? v.deliveryCost,
      distance: src.distance ?? v.distance,
      status: src.status ?? v.status,
      assignedDispatcherId: src.assignedDispatcherId ?? v.assignedDispatcherId,
      assignedDispatcherName: src.assignedDispatcherName ?? v.assignedDispatcherName,
      tripId: src.tripId ?? null,
      tripNumber: src.tripNumber ?? null,
      requestedPickupDate: src.requestedPickupDate ? new Date(src.requestedPickupDate) : null,
      requestedDeliveryDate: src.requestedDeliveryDate ? new Date(src.requestedDeliveryDate) : null,
      container: src.container ?? null,
      originTerminal: src.originTerminal ?? null,
      destinationTerminal: src.destinationTerminal ?? null,
      hazmatClass: src.hazmatClass ?? null,
      notes: src.notes ?? "",
      unNumber: src.unNumber ?? "",
      isHazmat: src.isHazmat ?? false,
    }));

    if (src.originLocation) {
      this.originCoords.set({
        id: "origin",
        location: {
          longitude: src.originLocation.longitude,
          latitude: src.originLocation.latitude,
        },
      });
    }
    if (src.destinationLocation) {
      this.destinationCoords.set({
        id: "destination",
        location: {
          longitude: src.destinationLocation.longitude,
          latitude: src.destinationLocation.latitude,
        },
      });
    }
  }

  private setCurrentDispatcher(): void {
    const userData = this.authService.getUserData();

    if (userData) {
      this.model.update((v) => ({
        ...v,
        assignedDispatcherId: userData.id,
        assignedDispatcherName: userData.getFullName(),
      }));
    }
  }
}
