import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { SelectModule } from "primeng/select";
import { InputTextModule } from "primeng/inputtext";
import { PasswordModule } from "primeng/password";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import {
  Api,
  createEldProvider,
  getEldProviders,
  type EldProviderConfigurationDto,
  type EldProviderType,
} from "@/core/api";
import { FormField } from "@/shared/components";

interface ProviderOption {
  label: string;
  value: EldProviderType;
  description: string;
}

@Component({
  selector: "app-eld-providers",
  templateUrl: "./eld-providers.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePipe,
    DialogModule,
    FormField,
    InputTextModule,
    PasswordModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class EldProvidersComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly fb = inject(FormBuilder);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly providers = signal<EldProviderConfigurationDto[]>([]);
  protected readonly showAddDialog = signal(false);

  protected readonly providerOptions: ProviderOption[] = [
    {
      label: "Demo (Testing)",
      value: "demo" as EldProviderType,
      description: "Simulated ELD data for testing without real devices",
    },
    {
      label: "Samsara",
      value: "samsara",
      description: "Connect to Samsara ELD devices",
    },
    {
      label: "Motive (KeepTruckin)",
      value: "motive",
      description: "Connect to Motive/KeepTruckin ELD devices",
    },
  ];

  protected readonly addForm = this.fb.group({
    providerType: ["demo" as EldProviderType, Validators.required],
    apiKey: ["", Validators.required],
    apiSecret: [""],
    webhookSecret: [""],
  });

  ngOnInit(): void {
    this.loadProviders();
  }

  protected async loadProviders(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const data = await this.api.invoke(getEldProviders);
      this.providers.set(data ?? []);
    } catch (err) {
      this.error.set("Failed to load ELD providers");
      console.error("Error loading providers:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected openAddDialog(): void {
    this.addForm.reset({
      providerType: "demo" as EldProviderType,
      apiKey: "",
      apiSecret: "",
      webhookSecret: "",
    });
    this.showAddDialog.set(true);
  }

  protected async saveProvider(): Promise<void> {
    if (this.addForm.invalid) return;

    this.saving.set(true);
    try {
      const formValue = this.addForm.value;
      await this.api.invoke(createEldProvider, {
        body: {
          providerType: formValue.providerType as EldProviderType,
          apiKey: formValue.apiKey ?? "",
          apiSecret: formValue.apiSecret || undefined,
          webhookSecret: formValue.webhookSecret || undefined,
        },
      });
      this.showAddDialog.set(false);
      await this.loadProviders();
    } catch (err) {
      console.error("Error saving provider:", err);
    } finally {
      this.saving.set(false);
    }
  }

  protected getProviderLabel(type?: EldProviderType): string {
    if (!type) return "Unknown";
    const option = this.providerOptions.find((o) => o.value === type);
    return option?.label ?? type;
  }

  protected getStatusSeverity(isConnected?: boolean): "success" | "danger" | "secondary" {
    if (isConnected === undefined) return "secondary";
    return isConnected ? "success" : "danger";
  }

  protected goBack(): void {
    this.router.navigate(["/eld"]);
  }

  protected manageDriverMappings(providerId: string): void {
    this.router.navigate(["/eld/providers", providerId, "mappings"]);
  }
}
