import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, maxLength, minLength, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { COUNTRIES_OPTIONS, regionAllowedCountries } from "@logistics/shared";
import type { Address, TerminalType } from "@logistics/shared/api";
import { regionOptions, terminalTypeOptions } from "@logistics/shared/api/enums";
import {
  AddressForm,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiFormField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { TenantService } from "@/core/services/tenant.service";

export interface TerminalFormValue {
  name: string;
  code: string;
  countryCode: string;
  type: TerminalType;
  address: Address | null;
  notes: string | null;
}

// The editable model. `notes` is a plain string here (default "") so it binds to
// `ui-textarea-field` (a `FormValueControl<string>`); the public `TerminalFormValue.notes`
// stays `string | null` and the string is coerced at the emit boundary below.
const EMPTY = {
  name: "",
  code: "",
  countryCode: "",
  type: "sea_port" as TerminalType,
  address: null as Address | null,
  notes: "",
};

@Component({
  selector: "app-terminal-form",
  templateUrl: "./terminal-form.html",
  imports: [
    AddressForm,
    FormField,
    FormRoot,
    Grid,
    Icon,
    RouterLink,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiFormField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
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

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Name is required." });
      required(p.code, { message: "Code is required." });
      minLength(p.code, 5, { message: "Code must be exactly 5 characters." });
      maxLength(p.code, 5, { message: "Code must be exactly 5 characters." });
      required(p.countryCode, { message: "Country is required." });
      required(p.type, { message: "Type is required." });
      required(p.address, { message: "Address is required." });
    },
    {
      submission: {
        action: async () => {
          const v = this.model();
          this.save.emit({
            name: v.name,
            code: v.code.toUpperCase(),
            countryCode: v.countryCode.toUpperCase(),
            type: v.type,
            address: v.address,
            notes: v.notes,
          });
          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const initialData = this.initial();
      if (!initialData) return;
      if (this.mode() === "edit" && this.form().dirty()) return;
      this.model.set({
        name: initialData.name ?? "",
        code: initialData.code ?? "",
        countryCode: initialData.countryCode ?? "",
        type: initialData.type ?? "sea_port",
        address: initialData.address ?? null,
        notes: initialData.notes ?? "",
      });
    });
  }
}
