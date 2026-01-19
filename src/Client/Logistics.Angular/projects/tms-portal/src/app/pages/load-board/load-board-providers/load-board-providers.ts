import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import {
  Api,
  type LoadBoardConfigurationDto,
  type LoadBoardProviderType,
  createLoadBoardProvider,
  deleteLoadBoardProvider,
  getLoadBoardProviders,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { PasswordModule } from "primeng/password";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { LabeledField, PageHeader } from "@/shared/components";

interface ProviderOption {
  label: string;
  value: LoadBoardProviderType;
  description: string;
}

@Component({
  selector: "app-load-board-providers",
  templateUrl: "./load-board-providers.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePipe,
    DialogModule,
    InputTextModule,
    LabeledField,
    PageHeader,
    PasswordModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class LoadBoardProvidersComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly providers = signal<LoadBoardConfigurationDto[]>([]);
  protected readonly showAddDialog = signal(false);

  protected readonly providerOptions: ProviderOption[] = [
    {
      label: "Demo (Testing)",
      value: "demo" as LoadBoardProviderType,
      description: "Simulated load board data for testing",
    },
    {
      label: "DAT",
      value: "dat" as LoadBoardProviderType,
      description: "Connect to DAT Freight & Analytics",
    },
    {
      label: "Truckstop",
      value: "truckstop" as LoadBoardProviderType,
      description: "Connect to Truckstop.com",
    },
    {
      label: "123Loadboard",
      value: "one_two3_loadboard" as LoadBoardProviderType,
      description: "Connect to 123Loadboard",
    },
  ];

  protected readonly addForm = this.fb.group({
    providerType: ["demo" as LoadBoardProviderType, Validators.required],
    apiKey: ["", Validators.required],
    apiSecret: [""],
    companyDotNumber: [""],
    companyMcNumber: [""],
  });

  ngOnInit(): void {
    this.loadProviders();
  }

  protected async loadProviders(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const data = await this.api.invoke(getLoadBoardProviders);
      this.providers.set(data ?? []);
    } catch (err) {
      this.error.set("Failed to load load board providers");
      console.error("Error loading providers:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected openAddDialog(): void {
    this.addForm.reset({
      providerType: "demo" as LoadBoardProviderType,
      apiKey: "",
      apiSecret: "",
      companyDotNumber: "",
      companyMcNumber: "",
    });
    this.showAddDialog.set(true);
  }

  protected async saveProvider(): Promise<void> {
    if (this.addForm.invalid) return;

    this.saving.set(true);
    try {
      const formValue = this.addForm.value;
      await this.api.invoke(createLoadBoardProvider, {
        body: {
          providerType: formValue.providerType as LoadBoardProviderType,
          apiKey: formValue.apiKey ?? "",
          apiSecret: formValue.apiSecret,
          companyDotNumber: formValue.companyDotNumber,
          companyMcNumber: formValue.companyMcNumber,
        },
      });
      this.showAddDialog.set(false);
      this.toastService.showSuccess("Provider added successfully");
      await this.loadProviders();
    } catch (err) {
      console.error("Error saving provider:", err);
      this.toastService.showError("Failed to add provider");
    } finally {
      this.saving.set(false);
    }
  }

  protected confirmDeleteProvider(provider: LoadBoardConfigurationDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to delete the ${this.getProviderLabel(provider.providerType)} provider?`,
      header: "Delete Provider",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.deleteProvider(provider.id!),
    });
  }

  protected async deleteProvider(providerId: string): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(deleteLoadBoardProvider, { providerId });
      this.toastService.showSuccess("Provider deleted successfully");
      await this.loadProviders();
    } catch (err) {
      console.error("Error deleting provider:", err);
      this.error.set("Failed to delete provider");
    } finally {
      this.loading.set(false);
    }
  }

  protected getProviderLabel(type?: LoadBoardProviderType): string {
    if (!type) return "Unknown";
    const option = this.providerOptions.find((o) => o.value === type);
    return option?.label ?? type;
  }

  protected goBack(): void {
    this.router.navigate(["/loadboard"]);
  }
}
