import { DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { LabeledField } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import {
  Api,
  type EldDriverDto,
  type EldDriverMappingDto,
  type EldProviderType,
  type EmployeeDto,
  deleteEldDriverMapping,
  getEldDriverMappings,
  getEldProviderDrivers,
  getEldProviders,
  getEmployees,
  mapEldDriver,
} from "@/core/api";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-eld-driver-mappings",
  templateUrl: "./eld-driver-mappings.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePipe,
    FormsModule,
    LabeledField,
    ProgressSpinnerModule,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class EldDriverMappingsComponent implements OnInit {
  private readonly router = inject(Router);
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

  protected readonly unmappedEldDrivers = computed(() => {
    const mappedIds = new Set(this.mappings().map((m) => m.externalDriverId));
    return this.eldDrivers().filter((d) => !mappedIds.has(d.externalDriverId));
  });

  protected readonly unmappedEmployees = computed(() => {
    const mappedIds = new Set(this.mappings().map((m) => m.employeeId));
    return this.employees().filter((e) => !mappedIds.has(e.id!));
  });

  protected selectedEldDriver: EldDriverDto | null = null;
  protected selectedEmployee: EmployeeDto | null = null;

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    const providerId = this.providerId();

    this.loading.set(true);
    this.error.set(null);

    try {
      // Load provider info
      const providers = await this.api.invoke(getEldProviders);
      const provider = providers?.find((p) => p.id === providerId);
      if (provider) {
        this.providerName.set(this.getProviderLabel(provider.providerType));
        this.providerType.set(provider.providerType ?? null);
      }

      // Load data in parallel
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
    if (!this.selectedEldDriver || !this.selectedEmployee || !this.providerType()) {
      return;
    }

    this.saving.set(true);
    try {
      await this.api.invoke(mapEldDriver, {
        body: {
          employeeId: this.selectedEmployee.id!,
          providerType: this.providerType()!,
          externalDriverId: this.selectedEldDriver.externalDriverId!,
          externalDriverName: this.selectedEldDriver.name,
        },
      });

      this.selectedEldDriver = null;
      this.selectedEmployee = null;
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
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
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

  protected goBack(): void {
    this.router.navigate(["/eld/providers"]);
  }

  private getProviderLabel(type?: EldProviderType): string {
    const labels: Record<string, string> = {
      demo: "Demo (Testing)",
      samsara: "Samsara",
      motive: "Motive (KeepTruckin)",
      geotab: "Geotab",
      omnitracs: "Omnitracs",
      people_net: "PeopleNet",
    };
    return type ? (labels[type] ?? type) : "Unknown";
  }
}
