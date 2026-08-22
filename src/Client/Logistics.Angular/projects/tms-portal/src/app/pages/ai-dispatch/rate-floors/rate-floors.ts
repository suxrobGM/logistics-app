import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import {
  form,
  FormField,
  FormRoot,
  maxLength,
  min,
  minLength,
  required,
} from "@angular/forms/signals";
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
  Card,
  EmptyState,
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
import { TenantService, ToastService } from "@/core/services";
import { formatRateFloorLane } from "@/shared/utils";

interface LaneRateFloorModel {
  originCountry: string;
  originState: string;
  destinationCountry: string;
  destinationState: string;
  minRatePerMile: number | null;
  minTotalRateAmount: number | null;
  notes: string;
}

const EMPTY_FLOOR: LaneRateFloorModel = {
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
    Card,
    CurrencyFormatPipe,
    EmptyState,
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
  private readonly tenantService = inject(TenantService);

  protected readonly canManage = computed(() =>
    this.permissionService.hasPermission(Permission.Negotiation.Manage),
  );

  protected readonly isLoading = signal(false);
  protected readonly floors = signal<LaneRateFloorDto[]>([]);
  protected readonly editingId = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);

  protected readonly model = signal<LaneRateFloorModel>({ ...EMPTY_FLOOR });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.originCountry, { message: "Origin country is required." });
      minLength(p.originCountry, 2, { message: "Origin country must be a 2-letter code." });
      maxLength(p.originCountry, 2, { message: "Origin country must be a 2-letter code." });

      minLength(p.originState, 2, {
        when: ({ value }) => value().length > 0,
        message: "Origin state must be a 2-letter code.",
      });
      maxLength(p.originState, 2, { message: "Origin state must be a 2-letter code." });

      required(p.destinationCountry, { message: "Destination country is required." });
      minLength(p.destinationCountry, 2, {
        message: "Destination country must be a 2-letter code.",
      });
      maxLength(p.destinationCountry, 2, {
        message: "Destination country must be a 2-letter code.",
      });

      minLength(p.destinationState, 2, {
        when: ({ value }) => value().length > 0,
        message: "Destination state must be a 2-letter code.",
      });
      maxLength(p.destinationState, 2, { message: "Destination state must be a 2-letter code." });

      required(p.minRatePerMile, { message: "A minimum rate per mile is required." });
      min(p.minRatePerMile, 0.01, { message: "The minimum rate per mile must be greater than 0." });

      min(p.minTotalRateAmount, 0.01, {
        when: ({ value }) => value() !== null,
        message: "The minimum total rate must be greater than 0.",
      });
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
    this.form().reset({ ...EMPTY_FLOOR });
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
    return formatRateFloorLane(floor);
  }

  protected askDelete(floor: LaneRateFloorDto): void {
    const id = floor.id;
    if (!id) return;

    this.toastService.confirm({
      message: `Delete the rate floor for ${this.lane(floor)}?`,
      accept: () => this.delete(id),
    });
  }

  private async save(): Promise<void> {
    const value = this.model();
    const body = {
      originCountry: value.originCountry.trim(),
      originState: value.originState.trim() || null,
      destinationCountry: value.destinationCountry.trim(),
      destinationState: value.destinationState.trim() || null,
      minRatePerMile: value.minRatePerMile ?? undefined,
      minTotalRateAmount: value.minTotalRateAmount,
      minTotalRateCurrency: this.tenantService.tenantCurrency(),
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
    } catch (error) {
      // A duplicate lane comes back as a 400 naming the conflicting row - show it, it says what to fix.
      this.toastService.showError(getApiErrorMessage(error, "Failed to save the rate floor"));
    }
  }

  private async delete(id: string): Promise<void> {
    try {
      await this.api.invoke(deleteLaneRateFloor, { id });
      this.toastService.showSuccess("Rate floor deleted");
      await this.load();
    } catch (error) {
      this.toastService.showError(getApiErrorMessage(error, "Failed to delete the rate floor"));
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
