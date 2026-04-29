import { Component, computed, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { COUNTRIES_OPTIONS } from "@logistics/shared";
import type { Address, TerminalType } from "@logistics/shared/api";
import { regionOptions, terminalTypeOptions } from "@logistics/shared/api/enums";
import { AddressForm, LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { Select } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { TenantService } from "@/core/services/tenant.service";
import { regionAllowedCountries } from "@/shared/utils";

export interface TerminalFormValue {
  name: string;
  code: string;
  countryCode: string;
  type: TerminalType;
  address: Address | null;
  notes: string | null;
}

@Component({
  selector: "app-terminal-form",
  templateUrl: "./terminal-form.html",
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    Select,
    TextareaModule,
    AddressForm,
    LabeledField,
    ValidationSummary,
  ],
})
export class TerminalForm {
  private readonly tenantService = inject(TenantService);

  protected readonly typeOptions = terminalTypeOptions;
  protected readonly regionOptions = regionOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TerminalFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TerminalFormValue>();

  protected readonly tenantRegion = computed(
    () => this.tenantService.tenantData()?.settings?.region ?? "us",
  );

  protected readonly allowedCountries = computed(() => regionAllowedCountries(this.tenantRegion()));

  protected readonly countryOptions = computed(() => {
    const allowed = new Set(this.allowedCountries());
    return COUNTRIES_OPTIONS.filter((opt) => allowed.has(opt.value));
  });

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    code: new FormControl("", {
      validators: [Validators.required, Validators.minLength(5), Validators.maxLength(5)],
      nonNullable: true,
    }),
    countryCode: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    type: new FormControl<TerminalType>("sea_port", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    address: new FormControl<Address | null>(null, { validators: [Validators.required] }),
    notes: new FormControl<string | null>(null),
  });

  constructor() {
    effect(() => {
      const initialData = this.initial();
      if (!initialData) return;
      if (this.mode() === "edit" && this.form.dirty) return;
      this.form.patchValue({
        name: initialData.name ?? "",
        code: initialData.code ?? "",
        countryCode: initialData.countryCode ?? "",
        type: initialData.type ?? "sea_port",
        address: initialData.address ?? null,
        notes: initialData.notes ?? null,
      });
    });
  }

  protected submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    this.save.emit({
      name: v.name,
      code: v.code.toUpperCase(),
      countryCode: v.countryCode.toUpperCase(),
      type: v.type,
      address: v.address,
      notes: v.notes,
    });
  }
}
