import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { form, FormField, FormRoot, min, required } from "@angular/forms/signals";
import { getApiErrorMessage, PageHeader, Permission } from "@logistics/shared";
import {
  Api,
  createLaneRateFloor,
  deleteLaneRateFloor,
  getLaneRateFloors,
  updateLaneRateFloor,
  type LaneRateFloorDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Alert,
  Card,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
  UiDialog,
  UiFormField,
  UiNumberField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";
import { ToastService } from "@/core/services";

interface LaneRateFloorModel {
  originCountry: string;
  originState: string;
  destinationCountry: string;
  destinationState: string;
  minRatePerMile: number | null;
  minTotalRateAmount: number | null;
  notes: string;
}

const EMPTY: LaneRateFloorModel = {
  originCountry: "US",
  originState: "",
  destinationCountry: "US",
  destinationState: "",
  minRatePerMile: null,
  minTotalRateAmount: null,
  notes: "",
};

@Component({
  selector: "app-rate-floors",
  templateUrl: "./rate-floors.html",
  imports: [
    Alert,
    Card,
    CurrencyFormatPipe,
    FormField,
    FormRoot,
    PageHeader,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiTextField,
    ValidatedForm,
  ],
})
export class RateFloors implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly permissionService = inject(PermissionService);

  protected readonly canManage = computed(() =>
    this.permissionService.hasPermission(Permission.Negotiation.Manage),
  );

  protected readonly isLoading = signal(false);
  protected readonly floors = signal<LaneRateFloorDto[]>([]);
  protected readonly editingId = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);

  protected readonly model = signal<LaneRateFloorModel>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.originCountry, { message: "Origin country is required." });
      required(p.destinationCountry, { message: "Destination country is required." });
      required(p.minRatePerMile, { message: "A minimum rate per mile is required." });
      min(p.minRatePerMile, 0, { message: "The minimum rate cannot be negative." });
    },
    {
      submission: {
        action: async () => {
          await this.save();
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.load();
  }

  protected openCreate(): void {
    this.editingId.set(null);
    this.form().reset({ ...EMPTY });
    this.dialogOpen.set(true);
  }

  protected openEdit(floor: LaneRateFloorDto): void {
    if (!floor.id) return;
    this.editingId.set(floor.id);
    this.form().reset({
      originCountry: floor.originCountry ?? "US",
      originState: floor.originState ?? "",
      destinationCountry: floor.destinationCountry ?? "US",
      destinationState: floor.destinationState ?? "",
      minRatePerMile: floor.minRatePerMile ?? null,
      minTotalRateAmount: floor.minTotalRate?.amount ?? null,
      notes: floor.notes ?? "",
    });
    this.dialogOpen.set(true);
  }

  protected closeDialog(): void {
    this.dialogOpen.set(false);
  }

  protected lane(floor: LaneRateFloorDto): string {
    const origin = this.place(floor.originCountry, floor.originState);
    const destination = this.place(floor.destinationCountry, floor.destinationState);
    return `${origin} to ${destination}`;
  }

  protected askDelete(floor: LaneRateFloorDto): void {
    if (!floor.id) return;
    this.toastService.confirm({
      message: `Delete the rate floor for ${this.lane(floor)}?`,
      accept: () => this.delete(floor.id!),
    });
  }

  /** "Any state" is what a null state column means to the resolver, so it reads that way here too. */
  private place(country?: string | null, state?: string | null): string {
    return state ? `${country}-${state}` : `${country} (any state)`;
  }

  private async save(): Promise<void> {
    const value = this.model();
    const body = {
      originCountry: value.originCountry.trim(),
      originState: value.originState.trim() || null,
      destinationCountry: value.destinationCountry.trim(),
      destinationState: value.destinationState.trim() || null,
      minRatePerMile: value.minRatePerMile!,
      minTotalRateAmount: value.minTotalRateAmount,
      minTotalRateCurrency: "USD",
      notes: value.notes.trim() || null,
    };

    const editing = this.editingId();

    try {
      if (editing) {
        await this.api.invoke(updateLaneRateFloor, { id: editing, body });
        this.toastService.showSuccess("Rate floor updated");
      } else {
        await this.api.invoke(createLaneRateFloor, { body });
        this.toastService.showSuccess("Rate floor created");
      }
      this.closeDialog();
      await this.load();
    } catch (error: unknown) {
      // A duplicate lane comes back as a 400 naming the conflicting row - show it, it says what to fix.
      this.toastService.showError(getApiErrorMessage(error, "Failed to save the rate floor"));
    }
  }

  private async delete(id: string): Promise<void> {
    try {
      await this.api.invoke(deleteLaneRateFloor, { id });
      this.toastService.showSuccess("Rate floor deleted");
      await this.load();
    } catch {
      this.toastService.showError("Failed to delete the rate floor");
    }
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      this.floors.set((await this.api.invoke(getLaneRateFloors, {})) ?? []);
    } catch {
      this.toastService.showError("Failed to load rate floors");
    } finally {
      this.isLoading.set(false);
    }
  }
}
