import { DatePipe } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import {
  Api,
  deleteEldDriverMapping,
  getEldDriverMappings,
  getEldProviderDrivers,
  getEldProviders,
  getEmployees,
  mapEldDriver,
  type EldDriverDto,
  type EldDriverMappingDto,
  type EldProviderType,
  type EmployeeDto,
} from "@logistics/shared/api";
import {
  Badge,
  DashboardCard,
  EmptyState,
  ErrorState,
  Grid,
  Icon,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiSelectField,
  UiTooltip,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader, UiFormField } from "@/shared/components";
import { getEldProviderLabel } from "../_components";

@Component({
  selector: "app-eld-driver-mappings",
  templateUrl: "./eld-driver-mappings.html",
  imports: [
    Badge,
    DashboardCard,
    DatePipe,
    EmptyState,
    ErrorState,
    Grid,
    Icon,
    PageHeader,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    UiFormField,
    UiSelectField,
    UiTooltip,
  ],
})
export class EldDriverMappingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  readonly providerId = input.required<string>();

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly providerName = signal<string>("ELD");
  protected readonly providerType = signal<EldProviderType | null>(null);

  protected readonly mappings = signal<EldDriverMappingDto[]>([]);
  protected readonly eldDrivers = signal<EldDriverDto[]>([]);
  protected readonly employees = signal<EmployeeDto[]>([]);

  protected readonly headerSubtitle = computed(
    () => `Map your TMS drivers to ${this.providerName()} ELD drivers`,
  );

  protected readonly mappingsCardTitle = computed(
    () => `Current Mappings (${this.mappings().length})`,
  );

  protected readonly unmappedEldDrivers = computed(() => {
    const mappedIds = new Set(this.mappings().map((m) => m.externalDriverId));
    return this.eldDrivers().filter((d) => !mappedIds.has(d.externalDriverId));
  });

  protected readonly unmappedEmployees = computed(() => {
    const mappedIds = new Set(this.mappings().map((m) => m.employeeId));
    return this.employees().filter((e) => !mappedIds.has(e.id!));
  });

  protected readonly selectedEldDriver = signal<EldDriverDto | null>(null);
  protected readonly selectedEmployee = signal<EmployeeDto | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    const providerId = this.providerId();

    this.loading.set(true);
    this.error.set(null);

    try {
      const providers = await this.api.invoke(getEldProviders);
      const provider = providers?.find((p) => p.id === providerId);
      if (provider) {
        this.providerName.set(getEldProviderLabel(provider.providerType));
        this.providerType.set(provider.providerType ?? null);
      }

      const [mappingsData, driversData, employeesData] = await Promise.all([
        this.api.invoke(getEldDriverMappings, { providerId }),
        this.api.invoke(getEldProviderDrivers, { providerId }),
        this.api.invoke(getEmployees, { Page: 1, PageSize: 100 }),
      ]);

      this.mappings.set(mappingsData ?? []);
      this.eldDrivers.set(driversData ?? []);
      this.employees.set(employeesData?.items ?? []);
    } catch (err) {
      this.error.set("Failed to load data");
      console.error("Error loading mappings:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async createMapping(): Promise<void> {
    const eldDriver = this.selectedEldDriver();
    const employee = this.selectedEmployee();
    if (!eldDriver || !employee || !this.providerType()) {
      return;
    }

    this.saving.set(true);
    try {
      await this.api.invoke(mapEldDriver, {
        body: {
          employeeId: employee.id!,
          providerType: this.providerType()!,
          externalDriverId: eldDriver.externalDriverId!,
          externalDriverName: eldDriver.name,
        },
      });

      this.selectedEldDriver.set(null);
      this.selectedEmployee.set(null);
      await this.loadData();
    } catch (err) {
      console.error("Error creating mapping:", err);
    } finally {
      this.saving.set(false);
    }
  }

  protected confirmDeleteMapping(mapping: EldDriverMappingDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to remove the mapping for ${mapping.employeeName}?`,
      header: "Remove Mapping",
      icon: "warning",
      severity: "danger",
      accept: () => this.deleteMapping(mapping.id!),
    });
  }

  protected async deleteMapping(mappingId: string): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(deleteEldDriverMapping, { mappingId });
      await this.loadData();
    } catch (err) {
      console.error("Error deleting mapping:", err);
      this.error.set("Failed to delete mapping");
    } finally {
      this.loading.set(false);
    }
  }
}
